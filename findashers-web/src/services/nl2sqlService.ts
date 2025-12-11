import axios from 'axios';

const API_BASE_URL = 'http://localhost:5144'; // Backend is running on port 5144

export interface NL2SQLQueryRequest {
  query: string;
  domain: string;
}

export interface NL2SQLQueryResponse {
  success: boolean;
  data: Array<Record<string, any>>;
  columnNames: string[];
  rowCount: number;
  sqlScript: string[];
  error: string;
  provider: string | null;
}

export class NL2SQLService {
  private static instance: NL2SQLService;
  private axiosInstance;

  private constructor() {
    this.axiosInstance = axios.create({
      baseURL: API_BASE_URL,
      headers: {
        'Content-Type': 'application/json',
      },
    });
  }

  public static getInstance(): NL2SQLService {
    if (!NL2SQLService.instance) {
      NL2SQLService.instance = new NL2SQLService();
    }
    return NL2SQLService.instance;
  }

  public async processQuery(query: string, domain: string = 'payments'): Promise<NL2SQLQueryResponse> {
    try {
      const response = await this.axiosInstance.post<NL2SQLQueryResponse>(
        '/api/query/query',
        { query, domain } as NL2SQLQueryRequest
      );
      return response.data;
    } catch (error) {
      console.error('Error processing NL2SQL query:', error);
      throw error;
    }
  }
}

export const nl2sqlService = NL2SQLService.getInstance();
