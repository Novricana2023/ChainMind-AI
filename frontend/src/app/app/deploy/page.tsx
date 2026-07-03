"use client";

import { useState } from "react";
import { Rocket } from "lucide-react";
import { AppLayout } from "@/components/layout/app-layout";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { ResultActions } from "@/components/shared/result-actions";
import { LoadingState } from "@/components/shared/loading-state";
import { api } from "@/lib/api";
import type { DeploymentResponse } from "@/types";

const NETWORKS = ["Ethereum", "Polygon", "Base", "Arbitrum"];

export default function DeployPage() {
  const [code, setCode] = useState("");
  const [network, setNetwork] = useState("Ethereum");
  const [result, setResult] = useState<DeploymentResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const handleDeploy = async () => {
    if (!code.trim()) return;
    setLoading(true);
    setError("");
    try {
      setResult(await api.deployContract(code, network));
    } catch (e) {
      setError(e instanceof Error ? e.message : "Failed");
    } finally {
      setLoading(false);
    }
  };

  const handleClear = () => {
    setCode("");
    setResult(null);
    setError("");
  };

  return (
    <AppLayout title="Deploy">
      <Card className="mb-6">
        <CardContent className="pt-6 space-y-4">
          <Textarea
            placeholder="Paste Solidity code..."
            value={code}
            onChange={(e) => setCode(e.target.value)}
            rows={8}
          />
          <div className="flex flex-wrap gap-2">
            {NETWORKS.map((n) => (
              <Button
                key={n}
                variant={network === n ? "default" : "secondary"}
                size="sm"
                onClick={() => setNetwork(n)}
              >
                {n}
              </Button>
            ))}
          </div>
          <Button onClick={handleDeploy} disabled={loading || !code.trim()}>
            <Rocket className="h-4 w-4" />
            {loading ? "Generating..." : "Generate Guide"}
          </Button>
          {error && <p className="text-red-400 text-sm">{error}</p>}
        </CardContent>
      </Card>

      {loading && <LoadingState />}
      {result && (
        <>
          <ResultActions onClear={handleClear} onRegenerate={handleDeploy} clearing={loading} />
          <div className="space-y-4">
            {result.steps.map((step, i) => (
              <Card key={i}>
                <CardHeader>
                  <CardTitle className="text-base">
                    <span className="text-purple-400 font-mono text-sm mr-2">{i + 1}.</span>
                    {step.title}
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  <p className="text-gray-400 text-sm mb-3">{step.description}</p>
                  {step.command && (
                    <pre className="p-3 rounded-lg bg-black/60 border border-white/10 text-sm font-mono text-cyan-300 overflow-x-auto">
                      {step.command}
                    </pre>
                  )}
                </CardContent>
              </Card>
            ))}
            <Card>
              <CardHeader><CardTitle>Verification</CardTitle></CardHeader>
              <CardContent><p className="text-gray-300 text-sm">{result.verificationGuide}</p></CardContent>
            </Card>
          </div>
        </>
      )}
    </AppLayout>
  );
}
