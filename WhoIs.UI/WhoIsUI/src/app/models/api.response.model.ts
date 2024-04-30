export interface ApiResponse<T> {
    success: boolean;
    errors: any[];
    errorCode: string
    exceptionMessage: string;
    data: T;
}