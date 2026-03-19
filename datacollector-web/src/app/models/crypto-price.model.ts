export interface CryptoPrice {
  id: number;
  coinId: string;
  symbol: string;
  name: string;
  priceUsd: number;
  change24h: number;
  marketCap: number;
  volume24h: number;
  collectedAt: string;
}

export interface PagedResponse {
  data: CryptoPrice[];
  page: number;
  pageSize: number;
  total: number;
  lastCollectedAt: string | null;
}

export interface StatsResponse {
  totalRecords: number;
  lastCollectedAt: string | null;
  checkedAt: string;
}
