"use client";

import { useState } from "react";
import { BookOpen } from "lucide-react";
import { AppLayout } from "@/components/layout/app-layout";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { ResultActions } from "@/components/shared/result-actions";
import { LoadingState } from "@/components/shared/loading-state";
import { api } from "@/lib/api";
import type { ExplainContractResponse } from "@/types";

export default function ExplainerPage() {
  const [code, setCode] = useState("");
  const [result, setResult] = useState<ExplainContractResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const handleExplain = async () => {
    if (!code.trim()) return;
    setLoading(true);
    setError("");
    try {
      setResult(await api.explainContract(code));
    } catch (e) {
      setError(e instanceof Error ? e.message : "Explanation failed");
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
    <AppLayout title="Explainer">
      <Card className="mb-6">
        <CardContent className="pt-6 space-y-4">
          <Textarea
            placeholder="Paste Solidity code..."
            value={code}
            onChange={(e) => setCode(e.target.value)}
            rows={10}
          />
          <Button onClick={handleExplain} disabled={loading || !code.trim()}>
            <BookOpen className="h-4 w-4" />
            {loading ? "Analyzing..." : "Explain"}
          </Button>
          {error && <p className="text-red-400 text-sm">{error}</p>}
        </CardContent>
      </Card>

      {loading && <LoadingState />}
      {result && (
        <>
          <ResultActions onClear={handleClear} onRegenerate={handleExplain} clearing={loading} />
          <div className="space-y-4">
            <Card>
              <CardHeader><CardTitle>Summary</CardTitle></CardHeader>
              <CardContent><p className="text-gray-300">{result.summary}</p></CardContent>
            </Card>
            <Card>
              <CardHeader><CardTitle>Business Purpose</CardTitle></CardHeader>
              <CardContent><p className="text-gray-300">{result.businessPurpose}</p></CardContent>
            </Card>
            <Card>
              <CardHeader><CardTitle>Explanation</CardTitle></CardHeader>
              <CardContent><p className="text-gray-300">{result.beginnerExplanation}</p></CardContent>
            </Card>
            <Card>
              <CardHeader><CardTitle>Security</CardTitle></CardHeader>
              <CardContent>
                <ul className="space-y-2">
                  {result.securityConsiderations.map((s, i) => (
                    <li key={i} className="flex items-start gap-2 text-gray-300 text-sm">
                      <Badge variant="medium" className="mt-0.5 shrink-0">!</Badge>
                      {s}
                    </li>
                  ))}
                </ul>
              </CardContent>
            </Card>
          </div>
        </>
      )}
    </AppLayout>
  );
}
