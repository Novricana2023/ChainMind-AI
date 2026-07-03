"use client";

import { useState } from "react";
import { Shield } from "lucide-react";
import { AppLayout } from "@/components/layout/app-layout";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Badge, severityVariant } from "@/components/ui/badge";
import { ResultActions } from "@/components/shared/result-actions";
import { LoadingState } from "@/components/shared/loading-state";
import { api } from "@/lib/api";
import type { AuditContractResponse } from "@/types";

export default function AuditorPage() {
  const [code, setCode] = useState("");
  const [result, setResult] = useState<AuditContractResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const handleAudit = async () => {
    if (!code.trim()) return;
    setLoading(true);
    setError("");
    try {
      setResult(await api.auditContract(code));
    } catch (e) {
      setError(e instanceof Error ? e.message : "Audit failed");
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
    <AppLayout title="Auditor">
      <Card className="mb-6">
        <CardContent className="pt-6 space-y-4">
          <Textarea
            placeholder="Paste Solidity code..."
            value={code}
            onChange={(e) => setCode(e.target.value)}
            rows={10}
          />
          <Button onClick={handleAudit} disabled={loading || !code.trim()}>
            <Shield className="h-4 w-4" />
            {loading ? "Auditing..." : "Audit"}
          </Button>
          {error && <p className="text-red-400 text-sm">{error}</p>}
        </CardContent>
      </Card>

      {loading && <LoadingState />}
      {result && (
        <>
          <ResultActions onClear={handleClear} onRegenerate={handleAudit} clearing={loading} />
          <div className="space-y-6">
            <div className="flex items-center gap-6">
              <div className="text-center">
                <div className="text-4xl font-bold gradient-text">{result.securityScore}</div>
                <p className="text-sm text-gray-400">Score</p>
              </div>
              <p className="text-gray-300 flex-1">{result.overallAssessment}</p>
            </div>
            <div className="space-y-4">
              {result.findings.map((f, i) => (
                <Card key={i}>
                  <CardHeader>
                    <div className="flex items-center gap-3">
                      <Badge variant={severityVariant(f.severity)}>{f.severity}</Badge>
                      <Badge variant="default">{f.category}</Badge>
                    </div>
                    <CardTitle className="text-base">{f.title}</CardTitle>
                    <CardDescription>{f.description}</CardDescription>
                  </CardHeader>
                  <CardContent>
                    <p className="text-sm text-emerald-300">{f.recommendation}</p>
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
