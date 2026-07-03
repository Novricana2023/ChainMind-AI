"use client";

import { useState } from "react";
import { FileText } from "lucide-react";
import { AppLayout } from "@/components/layout/app-layout";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import { Card, CardContent } from "@/components/ui/card";
import { ResultActions } from "@/components/shared/result-actions";
import { LoadingState } from "@/components/shared/loading-state";
import { api } from "@/lib/api";

export default function DocsPage() {
  const [code, setCode] = useState("");
  const [result, setResult] = useState<{ markdown: string; title: string } | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const handleGenerate = async () => {
    if (!code.trim()) return;
    setLoading(true);
    setError("");
    try {
      setResult(await api.generateDocs(code));
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
    <AppLayout title="Docs">
      <Card className="mb-6">
        <CardContent className="pt-6 space-y-4">
          <Textarea
            placeholder="Paste Solidity code..."
            value={code}
            onChange={(e) => setCode(e.target.value)}
            rows={10}
          />
          <Button onClick={handleGenerate} disabled={loading || !code.trim()}>
            <FileText className="h-4 w-4" />
            {loading ? "Generating..." : "Generate"}
          </Button>
          {error && <p className="text-red-400 text-sm">{error}</p>}
        </CardContent>
      </Card>

      {loading && <LoadingState />}
      {result && (
        <>
          <ResultActions onClear={handleClear} onRegenerate={handleGenerate} clearing={loading} />
          <Card>
            <CardContent className="pt-6">
              <h2 className="text-xl font-bold mb-4 gradient-text">{result.title}</h2>
              <pre className="whitespace-pre-wrap text-gray-300 font-sans text-sm leading-relaxed">
                {result.markdown}
              </pre>
            </CardContent>
          </Card>
        </>
      )}
    </AppLayout>
  );
}
