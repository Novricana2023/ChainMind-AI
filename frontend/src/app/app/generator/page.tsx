"use client";

import { useState } from "react";
import { Sparkles } from "lucide-react";
import { AppLayout } from "@/components/layout/app-layout";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { CodeViewer } from "@/components/shared/code-viewer";
import { ResultActions } from "@/components/shared/result-actions";
import { LoadingState } from "@/components/shared/loading-state";
import { api } from "@/lib/api";
import { PROMPT_TEMPLATES } from "@/lib/prompt-templates";

export default function GeneratorPage() {
  const [prompt, setPrompt] = useState("");
  const [activeTemplate, setActiveTemplate] = useState<string | null>(null);
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
    setActiveTemplate(null);
    setResult(null);
    setError("");
  };

  const applyTemplate = (id: string, templatePrompt: string) => {
    setActiveTemplate(id);
    setPrompt(templatePrompt);
    setResult(null);
    setError("");
  };

  return (
    <AppLayout title="Generator">
      <Card className="mb-6">
        <CardContent className="pt-6 space-y-4">
          <div className="space-y-2">
            <p className="text-sm text-muted-foreground">
              Start from a template or describe your contract. Detailed prompts produce better code.
            </p>
            <div className="flex flex-wrap gap-2">
              {PROMPT_TEMPLATES.map((t) => (
                <button
                  key={t.id}
                  type="button"
                  onClick={() => applyTemplate(t.id, t.prompt)}
                  className="focus:outline-none focus-visible:ring-2 focus-visible:ring-purple-500 rounded-full"
                >
                  <Badge
                    variant={activeTemplate === t.id ? "success" : "default"}
                    className="cursor-pointer hover:bg-purple-500/30 transition-colors px-3 py-1"
                  >
                    {t.label}
                  </Badge>
                </button>
              ))}
            </div>
          </div>

          <Textarea
            placeholder="Describe your contract requirements..."
            value={prompt}
            onChange={(e) => {
              setPrompt(e.target.value);
              setActiveTemplate(null);
            }}
            rows={8}
            className="text-sm font-mono"
          />
          <Button onClick={handleGenerate} disabled={loading || !prompt.trim()}>
            <Sparkles className="h-4 w-4" />
            {loading ? "Generating & reviewing..." : "Generate"}
          </Button>
          {error && <p className="text-red-400 text-sm">{error}</p>}
        </CardContent>
      </Card>

      {loading && <LoadingState message="Generating contract and running security review..." />}
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
