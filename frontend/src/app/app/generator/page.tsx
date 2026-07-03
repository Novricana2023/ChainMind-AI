"use client";

import { useState } from "react";
import { Sparkles } from "lucide-react";
import { AppLayout } from "@/components/layout/app-layout";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import { Card, CardContent } from "@/components/ui/card";
import { CodeViewer } from "@/components/shared/code-viewer";
import { ResultActions } from "@/components/shared/result-actions";
import { LoadingState } from "@/components/shared/loading-state";
import { api } from "@/lib/api";

export default function GeneratorPage() {
  const [prompt, setPrompt] = useState("");
  const [result, setResult] = useState<{ contractCode: string; contractName: string } | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const handleGenerate = async () => {
    if (!prompt.trim()) return;
    setLoading(true);
    setError("");
    try {
      setResult(await api.generateContract(prompt));
    } catch (e) {
      setError(e instanceof Error ? e.message : "Generation failed");
    } finally {
      setLoading(false);
    }
  };

  const handleClear = () => {
    setPrompt("");
    setResult(null);
    setError("");
  };

  return (
    <AppLayout title="Generator">
      <Card className="mb-6">
        <CardContent className="pt-6 space-y-4">
          <Textarea
            placeholder="Describe your contract..."
            value={prompt}
            onChange={(e) => setPrompt(e.target.value)}
            rows={4}
            className="text-base"
          />
          <Button onClick={handleGenerate} disabled={loading || !prompt.trim()}>
            <Sparkles className="h-4 w-4" />
            {loading ? "Generating..." : "Generate"}
          </Button>
          {error && <p className="text-red-400 text-sm">{error}</p>}
        </CardContent>
      </Card>

      {loading && <LoadingState />}
      {result && (
        <>
          <ResultActions
            onClear={handleClear}
            onRegenerate={handleGenerate}
            regenerateLabel="Regenerate"
            clearing={loading}
          />
          <CodeViewer
            code={result.contractCode}
            language="solidity"
            filename={`${result.contractName}.sol`}
          />
        </>
      )}
    </AppLayout>
  );
}
