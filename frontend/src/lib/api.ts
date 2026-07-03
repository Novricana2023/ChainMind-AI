const API_BASE = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5080";

async function fetchApi<T>(endpoint: string, options?: RequestInit): Promise<T> {
  const res = await fetch(`${API_BASE}${endpoint}`, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...options?.headers,
    },
  });

  if (!res.ok) {
    const error = await res.json().catch(() => ({ error: "Request failed" }));
    throw new Error(error.error || `HTTP ${res.status}`);
  }

  return res.json();
}

export const api = {
  generateContract: (prompt: string) =>
    fetchApi<{ contractCode: string; contractName: string }>("/api/contracts/generate", {
      method: "POST",
      body: JSON.stringify({ prompt }),
    }),

  explainContract: (contractCode: string) =>
    fetchApi<import("@/types").ExplainContractResponse>("/api/contracts/explain", {
      method: "POST",
      body: JSON.stringify({ contractCode }),
    }),

  auditContract: (contractCode: string) =>
    fetchApi<import("@/types").AuditContractResponse>("/api/contracts/audit", {
      method: "POST",
      body: JSON.stringify({ contractCode }),
    }),

  deployContract: (contractCode: string, network: string) =>
    fetchApi<import("@/types").DeploymentResponse>("/api/contracts/deploy", {
      method: "POST",
      body: JSON.stringify({ contractCode, network }),
    }),

  generateTests: (contractCode: string) =>
    fetchApi<import("@/types").GenerateTestsResponse>("/api/contracts/tests", {
      method: "POST",
      body: JSON.stringify({ contractCode }),
    }),

  analyzeGas: (contractCode: string) =>
    fetchApi<import("@/types").GasAnalysisResponse>("/api/contracts/gas", {
      method: "POST",
      body: JSON.stringify({ contractCode }),
    }),

  runAgent: (contractCode: string, additionalContext?: string) =>
    fetchApi<import("@/types").AgentModeResponse>("/api/contracts/agent", {
      method: "POST",
      body: JSON.stringify({ contractCode, additionalContext }),
    }),

  generateDocs: (contractCode: string) =>
    fetchApi<import("@/types").GenerateDocumentationResponse>("/api/contracts/documentation", {
      method: "POST",
      body: JSON.stringify({ contractCode }),
    }),

  getStats: () => fetchApi<import("@/types").DashboardStats>("/api/dashboard/stats"),
  getProjects: () => fetchApi<import("@/types").ProjectItem[]>("/api/dashboard/projects"),
  getTemplates: () => fetchApi<import("@/types").TemplateItem[]>("/api/dashboard/templates"),
  getSessions: () => fetchApi<import("@/types").SessionItem[]>("/api/dashboard/sessions"),
};
