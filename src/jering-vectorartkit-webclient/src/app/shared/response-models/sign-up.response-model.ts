 

export interface SignUpResponseModel {
	username?: string;
	modelState?: { [key: string]: any; };
	expectedError?: boolean;
	errorMessage?: string;
}