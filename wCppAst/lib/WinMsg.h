
//#define _UNICODE
//#define  UNICODE

#include <windows.h>
#include <stdio.h>
#define GETMODULEHANDLE			(GetModuleHandle(NULL))
#define SERVERWNDCLASSNAME		"WMSERVERWND#02"
//#define SERVERWNDCLASSNAME		L"WMSERVERWND#01"
#define	_WM_MAXMESSAGE_SIZE		0x10000
#define _WM_HEADER_SIZE	(2*sizeof(DWORD)+sizeof(HWND))


typedef struct _WM_DATASTRUCTURE{

	int iMessage;//Int32
	int cbSize;
	char* Data;
}*LPWM_DATASTRUCTURE;



LRESULT CALLBACK WMCOPYWNDPROC(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam) {

  LPWM_DATASTRUCTURE lpMsg;
  if(uMsg == WM_COPYDATA) {

     lpMsg = (LPWM_DATASTRUCTURE)((COPYDATASTRUCT*)lParam);

      printf("\niMessage: %d", lpMsg->iMessage);fflush(stdout);
      printf("\ncbSize: %d", lpMsg->cbSize);fflush(stdout);
printf("\nSS#######################################");fflush(stdout);
	printf("\nData: %s", lpMsg->Data);fflush(stdout);
printf("\nMMM#######################################");fflush(stdout);
     oCmdLineParser->fParseLine(lpMsg->Data);
printf("\nEE######################################");fflush(stdout);
  }
  else
   return DefWindowProc(hwnd, uMsg, wParam, lParam);
   
   return 0;
}


    HWND hWnd = 0;
void fCreateWindows() {
    WNDCLASS WMClass;
	WMClass.style = CS_GLOBALCLASS;
	WMClass.lpfnWndProc = WMCOPYWNDPROC;




    WMClass.hInstance = GETMODULEHANDLE;
    WMClass.lpszClassName = SERVERWNDCLASSNAME;
    WMClass.cbClsExtra = 0;
    WMClass.cbWndExtra = 0;
    WMClass.hbrBackground = NULL;
    WMClass.hCursor = NULL;
    WMClass.hIcon = NULL;
    WMClass.lpszMenuName = NULL;


    if(RegisterClass(&WMClass) == 0){
        printf("Class registeration failed!\n");fflush(stdout);
		return;
    }

	hWnd =  CreateWindow((LPCTSTR)SERVERWNDCLASSNAME, NULL, 0, 0, 0, 0, 0, NULL, NULL, WMClass.hInstance, NULL);
	//printf("\nWMCOPYSERVER: Server launched successfully! %d", hWnd);

    printf("\ncwcAst:A:|Ready|%d|\n",hWnd);fflush(stdout);


	if(hWnd == 0) {
		printf("\nWMCOPYSERVER: Can't create window!");fflush(stdout);
		return;
	}
}


DWORD WINAPI Thread_MsgFunc( LPVOID lpParam ){
	MSG iMsg;
    int     Data = 0;
    Data = *((int*)lpParam);


	printf("\nThread Created\n: %d", Data);fflush(stdout);



	fCreateWindows();

   // while(GetMessage(&iMsg, hWnd, 0, 0) ){
    while(GetMessage(&iMsg, 0, 0, 0) ){
        TranslateMessage(&iMsg);
        DispatchMessage(&iMsg);
    }
    return 0;
}


void fCreateWindowsMsg() {

	BOOL bVal;



	int Data_Of_Thread = 1;
	HANDLE hwnd_Thread = CreateThread( NULL, 0,  Thread_MsgFunc, &Data_Of_Thread, 0, NULL);

   // if ( Handle_Of_Thread == NULL)
   //     ExitProcess(Data_Of_Thread);

		/*
    	while(PeekMessage(&iMsg, 0, 0, 0,0) ){

			DispatchMessage(&iMsg);
		}
        */

	//printf("\nWMCOPYSERVER: Press Enter to quit...");
	//return getchar();
}
