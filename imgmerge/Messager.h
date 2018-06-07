#pragma once
#include "stdafx.h"

class Messager
{
public:
	Messager();
	~Messager();

	void sendStr(string str);

	HANDLE hPipe;             
};

