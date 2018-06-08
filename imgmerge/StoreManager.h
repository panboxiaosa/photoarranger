#pragma once
#include "stdafx.h"
#include "Fragment.h"


class StoreManager
{
public:
	StoreManager();
	~StoreManager();

	void saveTo(wstring tar);

	void saveDefault();

	void loadDefault();

	void load(wstring tar);

	void build(string tar);

private:
	vector<Fragment> fragments;
	int pixelWidth;
	int pixelHeight;
	int dpi;
	
};

