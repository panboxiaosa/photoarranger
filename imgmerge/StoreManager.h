#pragma once
#include "stdafx.h"
#include "Fragment.h"


class StoreManager
{
public:
	StoreManager();
	~StoreManager();

	void setTag(TIFF** tiff);

	void load(wstring tar);

	void build(wstring tar);

	byte* buildPart(int index, int height);

	void buildByStep(wstring tar);

	vector<wstring> getOriginFiles();

	bool between(Fragment& item, int offset, int height);

	void cutPast(Mat& prepare, int height, int offset, Fragment& frag, Mat& drawBoard);

	Mat createText(Fragment& frag);

private:

	vector<Fragment> fragments;
	int pixelWidth;
	int pixelHeight;
	int dpi;
	int margin;
	
};

