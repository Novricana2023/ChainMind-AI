"use client";

import { useState } from "react";
import { Fuel } from "lucide-react";
import { AppLayout } from "@/components/layout/app-layout";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { ResultActions } from "@/components/shared/result-actions";
import { LoadingState } from "@/components/shared/loading-state";
import { api } from "@/lib/api";
import type { GasAnalysisResponse } from "@/types";

export default function GasPage() {
  const [code, setCode] = useState("");
  const [result, setResult] = useState<GasAnalysisResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const handleAnalyze = async () => {
    if (!code.trim()) return;
    setLoading(true);
    setError("");
    try {
      setResult(await api.analyzeGas(code));
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
    <AppLayout title="Gas">
      <Card className="mb-6">
        <CardContent className="pt-6 space-y-4">
          <Textarea
            placeholder="Paste Solidity code..."
            value={code}
            onChange={(e) => setCode(e.target.value)}
            rows={10}
          />
          <Button onClick={handleAnalyze} disabled={loading || !code.trim()}>
            <Fuel className="h-4 w-4" />
            {loading ? "Analyzing..." : "Analyze"}
          </Button>
          {error && <p className="text-red-400 text-sm">{error}</p>}
        </CardContent>
      </Card>

      {loading && <LoadingState />}
      {result && (
        <>
          <ResultActions onClear={handleClear} onRegenerate={handleAnalyze} clearing={loading} />
          <div className="space-y-6">
            <div className="grid sm:grid-cols-2 gap-4">
              <Card>
                <CardContent className="pt-6 text-center">
                  <div className="text-4xl font-bold gradient-text">{result.efficiencyScore}%</div>
                  <p className="text-sm text-gray-400 mt-1">Efficiency</p>
                </CardContent>
              </Card>
              <Card>
                <CardContent className="pt-6">
                  <p className="text-sm text-gray-400 mb-1">Gas Usage</p>
                  <p className="text-sm text-gray-200 font-mono">{result.estimatedGasUsage}</p>
                </CardContent>
              </Card>
            </div>
            <Card>
              <CardHeader><CardTitle>Summary</CardTitle></CardHeader>
              <CardContent><p className="text-gray-300">{result.summary}</p></CardContent>
            </Card>
            <div className="space-y-3">
              {result.optimizations.map((opt, i) => (
                <Card key={i}>
                  <CardContent className="pt-6">
                    <div className="flex items-start justify-between gap-4">
                      <div>
                        <p className="font-medium text-sm">{opt.title}</p>
                        <p className="text-gray-400 text-sm mt-1">{opt.description}</p>
                      </div>
                      <Badge variant="success" className="shrink-0">{opt.estimatedSavings}</Badge>
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>
          </div>
        </>
      )}
    </AppLayout>
  );
}
