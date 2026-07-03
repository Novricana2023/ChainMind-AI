"use client";

import { useState } from "react";
import { Bot } from "lucide-react";
import { AppLayout } from "@/components/layout/app-layout";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge, severityVariant } from "@/components/ui/badge";
import { ResultActions } from "@/components/shared/result-actions";
import { LoadingState } from "@/components/shared/loading-state";
import { api } from "@/lib/api";
import type { AgentModeResponse } from "@/types";

export default function AgentPage() {
  const [code, setCode] = useState("");
  const [context, setContext] = useState("");
  const [result, setResult] = useState<AgentModeResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const handleRun = async () => {
    if (!code.trim()) return;
    setLoading(true);
    setError("");
    try {
      setResult(await api.runAgent(code, context || undefined));
    } catch (e) {
      setError(e instanceof Error ? e.message : "Review failed");
    } finally {
      setLoading(false);
    }
  };

  const handleClear = () => {
    setCode("");
    setContext("");
    setResult(null);
    setError("");
  };

  return (
    <AppLayout title="Agent">
      <Card className="mb-6">
        <CardContent className="pt-6 space-y-4">
          <Textarea
            placeholder="Paste Solidity code..."
            value={code}
            onChange={(e) => setCode(e.target.value)}
            rows={10}
          />
          <Textarea
            placeholder="Additional context (optional)"
            value={context}
            onChange={(e) => setContext(e.target.value)}
            rows={2}
            className="font-sans"
          />
          <Button onClick={handleRun} disabled={loading || !code.trim()}>
            <Bot className="h-4 w-4" />
            {loading ? "Reviewing..." : "Run Review"}
          </Button>
          {error && <p className="text-red-400 text-sm">{error}</p>}
        </CardContent>
      </Card>

      {loading && <LoadingState />}
      {result && (
        <>
          <ResultActions onClear={handleClear} onRegenerate={handleRun} clearing={loading} />
          <div className="space-y-6">
            <div className="flex items-center gap-6">
              <div className="text-center">
                <div className="text-4xl font-bold text-orange-400">{result.riskScore}</div>
                <p className="text-sm text-gray-400">Risk</p>
              </div>
              <p className="text-gray-300 flex-1">{result.executiveSummary}</p>
            </div>
            <Card>
              <CardHeader><CardTitle>Recommendations</CardTitle></CardHeader>
              <CardContent className="space-y-4">
                {result.recommendations.map((r, i) => (
                  <div key={i} className="p-4 rounded-lg bg-white/5 border border-white/10">
                    <div className="flex items-center gap-2 mb-2">
                      <Badge variant={severityVariant(r.priority)}>{r.priority}</Badge>
                      <Badge variant="default">{r.category}</Badge>
                    </div>
                    <p className="font-medium text-sm mb-2">{r.recommendation}</p>
                    <p className="text-gray-400 text-sm">{r.reasoning}</p>
                  </div>
                ))}
              </CardContent>
            </Card>
            <Card>
              <CardHeader><CardTitle>Report</CardTitle></CardHeader>
              <CardContent>
                <pre className="text-sm text-gray-300 whitespace-pre-wrap font-sans">{result.detailedReport}</pre>
              </CardContent>
            </Card>
          </div>
        </>
      )}
    </AppLayout>
  );
}
