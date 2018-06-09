#include "Messager.h"


Messager::Messager()
{
}

Messager::~Messager()
{
	
}


void Messager::sendStr(string str){

	Sleep(50);//�ȴ�pipe�Ĵ����ɹ��� 

	DWORD wlen = 0;
	BOOL bRet = WaitNamedPipe(TEXT("\\\\.\\Pipe\\MyPipe"), NMPWAIT_WAIT_FOREVER);

	if (!bRet)
	{
		printf("connect the namedPipe failed!\n");
		return;
	}


	HANDLE	hPipe = CreateFile(          //�ܵ�����һ��������ļ� 
			TEXT("\\\\.\\Pipe\\MyPipe"),    //�������ļ��� 
			GENERIC_READ | GENERIC_WRITE,   //�ļ�ģʽ 
			0,                              //�Ƿ��� 
			NULL,                           //ָ��һ��SECURITY_ATTRIBUTES�ṹ��ָ�� 
			OPEN_EXISTING,                  //�������� 
			FILE_ATTRIBUTE_NORMAL,          //�ļ�����(����,ֻ��)NORMALΪĬ������ 
			NULL);                          //ģ�崴���ļ��ľ�� 

		if (INVALID_HANDLE_VALUE == hPipe)
		{
			printf("open the exit pipe failed!\n");
			return;
		}
	
		char buf[256];

		char chr[4];
		char byteData[1024];
		DWORD dwData = sizeof(buf);
		byteData[3] = (dwData & 0xFF000000) >> 24;
		byteData[2] = (dwData & 0x00FF0000) >> 16;
		byteData[1] = (dwData & 0x0000FF00) >> 8;
		byteData[0] = (dwData & 0x000000FF);

		if (WriteFile(hPipe, byteData, 4, &wlen, 0) == FALSE) //��������������� 
		{
			printf("write to pipe failed!\n");
		}
		else
		{
			if (WriteFile(hPipe, str.c_str(), strlen(str.c_str()), &wlen, 0) == FALSE) //���������������
			{
				printf("write to pipe failed!\n");
			}
			else
			{
				if (strlen(buf) == wlen) {
					cout << "To Server: data " << endl;
				}
				
				char rbuf[256] = "";
				DWORD rlen = 0;

				ReadFile(hPipe, chr, 4, &rlen, 0);  //���ܷ����͹���������  
				dwData = chr[0] | chr[1] << 8 | chr[2] << 16 | chr[3] << 24;
				printf("%d\n", dwData);

				ReadFile(hPipe, rbuf, dwData, &rlen, 0); //���ܷ����͹���������  
				printf("From Server: data = %s, size = %d\n", rbuf, rlen);
			}
			CloseHandle(hPipe);
		}
	
}

