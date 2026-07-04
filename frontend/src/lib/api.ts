function resolveApiBase(): string {
  if (process.env.NEXT_PUBLIC_API_URL) {
    return process.env.NEXT_PUBLIC_API_URL.replace(/\/$/, "");
  }
  if (typeof window !== "undefined" && window.location.hostname.endsWith(".vercel.app")) {
    return "https://chainmind-api.onrender.com";
  }
  return "http://localhost:5080";
}

const API_BASE = resolveApiBase();
const REQUEST_TIMEOUT_MS = 120_000;

async function fetchApi<T>(endpoint: string, options?: RequestInit): Promise<T> {
  const controller = new AbortController();
  const timeout = setTimeout(() => controller.abort(), REQUEST_TIMEOUT_MS);

  try {
    const res = await fetch(`${API_BASE}${endpoint}`, {
      ...options,
      signal: controller.signal,
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
  } catch (err) {
    if (err instanceof Error && err.name === "AbortError") {
      throw new Error(
        "Request timed out. The API may be waking up (Render free tier) — wait 60 seconds and try again."
      );
    }
    if (err instanceof TypeError) {
      throw new Error(
        `Cannot reach API at ${API_BASE}. Check Render is running and NEXT_PUBLIC_API_URL is correct.`
      );
    }
    throw err;
  } finally {
    clearTimeout(timeout);
  }
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
