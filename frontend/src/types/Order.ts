export interface Order {
    id: string;
    client: string;
    date: string;
    total: number;
}

export type OrderCreationDto = Omit<Order, 'id'>;