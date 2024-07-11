#include "dxsharp.h"



namespace DXSharp
{
	namespace Helpers
	{
		void ExceptionManager::Assert(HRESULT res, String^ methodName)
		{
			if (FAILED(res))
				throw gcnew ArgumentException(String::Format("Call to {0} failed with HRESULT: {1}", methodName, res));
		}

		void Window::CreateDDrawContext()
		{
			IDirectDraw* dd;
			IDirectDrawSurface* pSurf; // Primary surface
			IDirectDrawSurface4* pSurf4; // 4th version of DD Surface interface
			IDirectDrawSurface* sSurf; // Render target surface
			IDirectDrawSurface4* sSurf4;
			Guard(DirectDrawCreate(0, &dd, 0));

			ddraw = dd;
			Guard(ddraw->SetCooperativeLevel(hwnd, DDSCL_NORMAL));

			// Create primary surface
			DDSURFACEDESC desc;
			memset(&desc, 0, sizeof(desc));
			desc.dwSize = sizeof(desc);
			desc.dwFlags = DDSD_CAPS;
			desc.ddsCaps.dwCaps = DDSCAPS_PRIMARYSURFACE;
			desc.dwBackBufferCount = 1;
			Guard(ddraw->CreateSurface(&desc, &pSurf, 0));

			Guard(pSurf->QueryInterface(IID_IDirectDrawSurface4, (LPVOID*)&pSurf4));
			primarySurface = pSurf4;

			DDPIXELFORMAT pf;
			pSurf->GetPixelFormat(&pf);

			// Create RT. Since primary surface is always covers all screen, back buffer should be of real size
			DDSURFACEDESC rtDesc;
			memset(&rtDesc, 0, sizeof(rtDesc));
			rtDesc.dwSize = sizeof(rtDesc);
			rtDesc.dwFlags = DDSD_CAPS | DDSD_WIDTH | DDSD_HEIGHT;
			rtDesc.ddsCaps.dwCaps = DDSCAPS_OFFSCREENPLAIN | DDSCAPS_3DDEVICE;
			rtDesc.dwWidth = Width;
			rtDesc.dwHeight = Height;
			Guard(ddraw->CreateSurface(&rtDesc, &sSurf, 0));
			Guard(sSurf->QueryInterface(IID_IDirectDrawSurface4, (LPVOID*)&sSurf4));

			// Create clipper
			IDirectDrawClipper* clipper;
			Guard(ddraw->CreateClipper(0, &clipper, 0));
			clipper->SetHWnd(0, hwnd);
			primarySurface->SetClipper(clipper);

			d3dSurface = sSurf4;
		}

		Window::Window(int width, int height, bool isFullScreen)
		{
			RECT clientRect;

			hwnd = CreateWindowA("static", "DDraw Window", WS_VISIBLE | WS_SYSMENU, 0, 0, width, height, 0, 0, 0, 0);
			GetClientRect(hwnd, &clientRect);

			Width = clientRect.right;
			Height = clientRect.bottom;
			CreateDDrawContext();
		}

		IntPtr Window::GetHandle()
		{
			return IntPtr(hwnd);
		}

		bool Window::DoEvents()
		{
			MSG msg;

			while (PeekMessage(&msg, hwnd, 0, 0, PM_REMOVE))
			{
				DefWindowProcA(msg.hwnd, msg.message, msg.wParam, msg.lParam);
			}

			return IsWindow(hwnd);
		}

		HRESULT CALLBACK OnTextureFormatSearchCallback(LPDDPIXELFORMAT fmt, LPVOID lpContext)
		{
			if (fmt->dwRGBBitCount == 16 && fmt->dwFlags == DDPF_RGB)
			{
				Console::WriteLine("Found opaque texture format: {0} {1}", fmt->dwRGBBitCount, fmt->dwFlags);

				Window::opaqueTextureFormat = new DDPIXELFORMAT();
				memcpy(Window::opaqueTextureFormat, fmt, sizeof(DDPIXELFORMAT));

				return DDENUMRET_OK;
			}

			return DDENUMRET_OK;
		}

		HRESULT CALLBACK OnDepthStencilFormatSearchCallback(LPDDPIXELFORMAT fmt, LPVOID lpContext)
		{
			if (fmt->dwFlags == DDPF_ZBUFFER && fmt->dwZBufferBitDepth == 16) // 16-bit Z-Buffer will be fine, since 24 and 32-bit ones consume much more memory
			{
				Console::WriteLine("Found Z-Buffer format: {0} {1}", fmt->dwZBufferBitDepth, fmt->dwFlags);

				Window::zBufferFormat = new DDPIXELFORMAT();
				memcpy(Window::zBufferFormat, fmt, sizeof(DDPIXELFORMAT));

				return DDENUMRET_OK;
			}

			return DDENUMRET_OK;
		}

		DXSharp::D3D::Device^ Window::CreateDevice()
		{
			IDirect3D3* d3d;
			IDirect3DDevice3* device;
			IDirectDrawSurface4* surf = d3dSurface;
			
			Guard(ddraw->QueryInterface(IID_IDirect3D3, (LPVOID*)&d3d));
			// Enumerate and pick best Z-Buffer format
			Guard(d3d->EnumZBufferFormats(IID_IDirect3DHALDevice, OnDepthStencilFormatSearchCallback, 0));

			// Create Z-Buffer for this device
			DDSURFACEDESC zbufDesc;
			memset(&zbufDesc, 0, sizeof(zbufDesc));
			zbufDesc.dwSize = sizeof(zbufDesc);
			zbufDesc.dwFlags = DDSD_CAPS | DDSD_WIDTH | DDSD_HEIGHT | DDSD_PIXELFORMAT;
			zbufDesc.ddsCaps.dwCaps = DDSCAPS_ZBUFFER | DDSCAPS_VIDEOMEMORY;
			memcpy(&zbufDesc.ddpfPixelFormat, Window::zBufferFormat, sizeof(zbufDesc.ddpfPixelFormat));
			zbufDesc.dwWidth = Width;
			zbufDesc.dwHeight = Height;

			IDirectDrawSurface* zTemp;
			IDirectDrawSurface4* zSurface;
			Guard(ddraw->CreateSurface(&zbufDesc, &zTemp, 0));
			Guard(zTemp->QueryInterface(IID_IDirectDrawSurface4, (LPVOID*)&zSurface));

			// Attach Z-Buffer to backbuffer
			Guard(d3dSurface->AddAttachedSurface(zSurface));
			Guard(d3d->CreateDevice(IID_IDirect3DHALDevice, surf, &device, 0));

			// Enumerate and pick best texture format
			device->EnumTextureFormats(OnTextureFormatSearchCallback, 0);

			return gcnew DXSharp::D3D::Device(d3d, device);
		}

		void Window::Present()
		{
			RECT rct = { };
			GetWindowRect(hwnd, &rct);

			Guard(primarySurface->Blt(&rct, d3dSurface, 0, 0, 0));
		}
	}
}