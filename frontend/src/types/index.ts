export interface GenerateContractResponse {
  contractCode: string;
  contractName: string;
  language: string;
}

export interface ExplainContractResponse {
  summary: string;
  businessPurpose: string;
  beginnerExplanation: string;
  securityConsiderations: string[];
}

export interface AuditFinding {
  severity: string;
  category: string;
  title: string;
  description: string;
  recommendation: string;
}

export interface AuditContractResponse {
  findings: AuditFinding[];
  securityScore: number;
  overallAssessment: string;
}

export interface DeploymentStep {
  title: string;
  description: string;
  command: string;
}

export interface DeploymentResponse {
  network: string;
  steps: DeploymentStep[];
  verificationGuide: string;
}

export interface GenerateTestsResponse {
  testCode: string;
  framework: string;
}

export interface GasOptimization {
  title: string;
  description: string;
  estimatedSavings: string;
}

export interface GasAnalysisResponse {
  efficiencyScore: number;
  estimatedGasUsage: string;
  optimizations: GasOptimization[];
  summary: string;
}

export interface AgentRecommendation {
  priority: string;
  category: string;
  recommendation: string;
  reasoning: string;
}

export interface AgentModeResponse {
  executiveSummary: string;
  recommendations: AgentRecommendation[];
  detailedReport: string;
  riskScore: number;
}

export interface GenerateDocumentationResponse {
  markdown: string;
  title: string;
}

export interface DashboardStats {
  contractsGenerated: number;
  auditsCompleted: number;
  securityScoreAverage: number;
  activeSessions: number;
}

export interface ProjectItem {
  id: string;
  name: string;
  type: string;
  createdAt: string;
  status: string;
}

export interface TemplateItem {
  id: string;
  name: string;
  description: string;
  category: string;
}

export interface SessionItem {
  id: string;
  title: string;
  type: string;
  createdAt: string;
}
