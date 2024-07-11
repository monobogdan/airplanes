#include "dxsharp.h"

#include "d3dutil.h"
#include <math.h>

float x = 0;

namespace DXSharp
{
	namespace D3D
	{
		Device::Device(IDirect3D3* direct3d, IDirect3DDevice3* device)
		{
			this->direct3d = direct3d;
			this->device = device;

			IDirect3DMaterial3* mat;

			Guard(direct3d->CreateMaterial(&mat, 0));
			material = mat;
		}

		void Device::AttachViewport(int width, int height, float clipX, float clipWidth, float clipY, float clipHeight, float maxZ)
		{
			IDirect3DViewport3* vp;
			D3DVIEWPORT2 viewport;
			memset(&viewport, 0, sizeof(viewport));

			viewport.dwSize = sizeof(viewport);
			viewport.dwWidth = width;
			viewport.dwHeight = height;
			viewport.dvClipX = clipX;
			viewport.dvClipY = clipY;
			viewport.dvClipWidth = clipWidth;
			viewport.dvClipHeight = clipHeight;
			viewport.dvMaxZ = maxZ;

			Guard(direct3d->CreateViewport(&vp, 0));
			Guard(device->AddViewport(vp));
			Guard(vp->SetViewport2(&viewport));

			Guard(device->SetCurrentViewport(vp));

			currentViewport = vp;
		}

		void Device::Clear(int count, Rect^ rct, ClearTarget flags, Color^ color, float zValue, int stencilValue)
		{
			D3DRECT rect = { rct->X, rct->Y, rct->Width, rct->Height };

			currentViewport->Clear2(count, &rect, (int)flags, color->GetRGBA(), zValue, stencilValue);
		}

		void Device::BeginScene()
		{
			Guard(device->BeginScene());

			device->SetRenderState(D3DRENDERSTATE_CULLMODE, D3DCULL_CW);
			device->SetRenderState(D3DRENDERSTATE_ZENABLE, D3DZB_TRUE);
			device->SetTextureStageState(0, D3DTSS_MIPFILTER, D3DTFP_LINEAR);
			device->SetTextureStageState(0, D3DTSS_MINFILTER, D3DFILTER_LINEAR);
			device->SetTextureStageState(0, D3DTSS_MAGFILTER, D3DFILTER_LINEAR);

			device->SetTextureStageState(1, D3DTSS_MIPFILTER, D3DTFP_LINEAR);
			device->SetTextureStageState(1, D3DTSS_MINFILTER, D3DFILTER_LINEAR);
			device->SetTextureStageState(1, D3DTSS_MAGFILTER, D3DFILTER_LINEAR);

			device->SetRenderState(D3DRENDERSTATE_BLENDENABLE, true);
			device->SetRenderState(D3DRENDERSTATE_SRCBLEND, D3DBLEND_SRCALPHA);
			device->SetRenderState(D3DRENDERSTATE_DESTBLEND, D3DBLEND_INVSRCALPHA);
			device->SetRenderState(D3DRENDERSTATE_DITHERENABLE, true);
			device->SetRenderState(D3DRENDERSTATE_SPECULARENABLE, true);

			device->SetRenderState(D3DRENDERSTATE_COLORKEYENABLE, true);
		}

		void Device::EndScene()
		{
			Guard(device->EndScene());
		}

		void Device::SetTexture(int stage, Texture^ tex)
		{
			Guard(device->SetTexture(stage, tex->texture));
		}

		void Device::SetRenderState(RenderState renderState, unsigned int value)
		{
			Guard(device->SetRenderState((D3DRENDERSTATETYPE)renderState, value));
		}

		void Device::SetRenderState(RenderState renderState, float value)
		{
			Guard(device->SetRenderState((D3DRENDERSTATETYPE)renderState, (DWORD)value));
		}

		void Device::SetMaterial(Material^ material)
		{
			if (material != nullptr)
			{
				D3DMATERIAL mat;
				memset(&mat, 0, sizeof(mat));
				mat.dwSize = sizeof(mat);
				mat.diffuse.r = material->DiffuseR;
				mat.diffuse.g = material->DiffuseG;
				mat.diffuse.b = material->DiffuseB;
				mat.diffuse.a = material->DiffuseA;

				mat.ambient.r = material->AmbientR;
				mat.ambient.g = material->AmbientG;
				mat.ambient.b = material->AmbientB;

				mat.emissive.r = material->EmissiveR;
				mat.emissive.g = material->EmissiveG;
				mat.emissive.b = material->EmissiveB;

				mat.dcvSpecular.r = material->SpecularR;
				mat.dcvSpecular.g = material->SpecularG;
				mat.dcvSpecular.b = material->SpecularB;

				mat.power = material->Power;
				
				Guard(this->material->SetMaterial(&mat));

				D3DMATERIALHANDLE handle;
				Guard(this->material->GetHandle(device, &handle));

				Guard(device->SetLightState(D3DLIGHTSTATE_MATERIAL, handle));
				device->SetLightState(D3DLIGHTSTATE_AMBIENT, RGB(255, 254, 242));
			}
		}

		void Device::SetTextureStageState(int stage, int state, int value)
		{
			Guard(device->SetTextureStageState(stage, (D3DTEXTURESTAGESTATETYPE)state, value));
		}

		void Device::SetTransform(TransformType transform, array<float>^ matrix)
		{
			D3DMATRIX m;
			D3DMATRIX identity;
			pin_ptr<float> arrPtr = &matrix[0];
			
			memcpy(&m._11, arrPtr, 16 * sizeof(float));
			float* f = (float*)&m._11;

			Guard(device->SetTransform((D3DTRANSFORMSTATETYPE)transform, &m));
		}

		void Device::AddLight(Light^ l)
		{
			if (l != nullptr)
			{
				Guard(currentViewport->AddLight(l->light));
			}
		}

		void Device::RemoveLight(Light^ l)
		{
			if (l != nullptr)
			{
				Guard(currentViewport->DeleteLight(l->light));
			}
		}

		void Device::Begin(PrimitiveType primitiveType, int vertexTypeDesc, bool lit)
		{
			Guard(device->Begin((D3DPRIMITIVETYPE)primitiveType, vertexTypeDesc, !lit ? D3DDP_DONOTLIGHT : 0));
		}

		void Device::Vertex(DXSharp::D3D::Vertex vertex)
		{
			pin_ptr<D3D::Vertex> ptr = &vertex;
			//Console::WriteLine("{0} {1} {2}", ptr->X, ptr->Y, ptr->Z);

			device->Vertex(ptr);
		}

		void Device::End()
		{
			Guard(device->End(0));
		}

		Light::Light(Device^ device)
		{
			IDirect3DLight* _light;
			Guard(device->direct3d->CreateLight(&_light, 0));

			light = _light;
		}

		Light::~Light()
		{
			light->Release();
		}

		void Light::Update()
		{
			D3DLIGHT2 lightDesc;
			memset(&lightDesc, 0, sizeof(lightDesc));
			lightDesc.dwSize = sizeof(lightDesc);
			lightDesc.dltType = (D3DLIGHTTYPE)Type;
			lightDesc.dcvColor.r = R;
			lightDesc.dcvColor.g = G;
			lightDesc.dcvColor.b = B;
			lightDesc.dvPosition.x = X;
			lightDesc.dvPosition.y = Y;
			lightDesc.dvPosition.z = Z;
			lightDesc.dvDirection.x = DX;
			lightDesc.dvDirection.y = DY;
			lightDesc.dvDirection.z = DZ;
			lightDesc.dvFalloff = FallOff;
			lightDesc.dvAttenuation0 = 1.0f;
			lightDesc.dvRange = D3DLIGHT_RANGE_MAX;
			lightDesc.dvAttenuation1 = LinearAttenuation;
			lightDesc.dvRange = Range;
			lightDesc.dvTheta = Theta;
			lightDesc.dvPhi = Phi;
			lightDesc.dwFlags = D3DLIGHT_ACTIVE;

			Guard(light->SetLight((LPD3DLIGHT)&lightDesc));
		}


		/* Structures */

		Color::Color(byte r, byte g, byte b, byte a)
		{
			R = r;
			G = g;
			B = b;
			A = a;
		}

		DWORD Color::GetRGBA()
		{
			return ((COLORREF)(((BYTE)(B) | ((WORD)((BYTE)(G)) << 8)) | (((DWORD)(BYTE)(R)) << 16) | (((DWORD)(BYTE)(A)) << 24)));
		}

		

		/* Texture */
		Texture::Texture(DXSharp::Helpers::Window^ window, int width, int height, int mipCount)
		{
			MipCount = mipCount;
			Width = width;
			Height = height;

			bool hasMips = mipCount > 1; // If texture has more than 1 mipmap, then create surface as complex, if not - then as single-level.

			DDSURFACEDESC2 desc;
			memset(&desc, 0, sizeof(desc));
			desc.dwSize = sizeof(desc);
			desc.dwFlags = DDSD_CAPS | DDSD_WIDTH | DDSD_HEIGHT | DDSD_PIXELFORMAT | DDSD_TEXTURESTAGE | DDSD_CKSRCBLT;
			desc.ddsCaps.dwCaps = DDSCAPS_TEXTURE | (hasMips ? (DDSCAPS_MIPMAP | DDSCAPS_COMPLEX) : 0);
			desc.ddsCaps.dwCaps2 = DDSCAPS2_TEXTUREMANAGE;
			desc.ddckCKSrcBlt.dwColorSpaceHighValue = 0;
			desc.ddckCKSrcBlt.dwColorSpaceLowValue = 0;
			memcpy(&desc.ddpfPixelFormat, DXSharp::Helpers::Window::opaqueTextureFormat, sizeof(desc.ddpfPixelFormat));
			desc.dwWidth = Width = width;
			desc.dwHeight = Height = height;

			IDirectDrawSurface4* surf;
			IDirect3DTexture2* tex;

			IDirectDraw4* dd2;
			window->ddraw->QueryInterface(IID_IDirectDraw4, (LPVOID*)&dd2);

			Guard(dd2->CreateSurface(&desc, &surf, 0));
			Guard(surf->QueryInterface(IID_IDirect3DTexture2, (LPVOID*)&tex));
			

			surface = surf;
			texture = tex;

			this->window = window;
		}

		IDirectDrawSurface4* Texture::AllocateTemporaryTexture(int width, int height)
		{
			DDSURFACEDESC2 desc;
			memset(&desc, 0, sizeof(desc));
			desc.dwSize = sizeof(desc);
			desc.ddpfPixelFormat.dwSize = sizeof(desc.ddpfPixelFormat);
			Guard(surface->GetSurfaceDesc(&desc));
			desc.dwFlags = DDSD_CAPS | DDSD_WIDTH | DDSD_HEIGHT | DDSD_PIXELFORMAT;
			desc.ddsCaps.dwCaps = DDSCAPS_TEXTURE | DDSCAPS_SYSTEMMEMORY;
			desc.ddsCaps.dwCaps2 = 0;
			desc.dwWidth = width;
			desc.dwHeight = height;

			IDirectDraw4* dd4;
			window->ddraw->QueryInterface(IID_IDirectDraw4, (LPVOID*)&dd4);

			IDirectDrawSurface4* tmpSurface;
			Guard(dd4->CreateSurface(&desc, &tmpSurface, 0));

			return tmpSurface;
		}

		void Texture::FromPixelArray(array<byte>^ pixels, int width, int height, int mipLevel)
		{
			if (pixels == nullptr)
				throw gcnew ArgumentException("Pixels can't be null");

			pin_ptr<byte> pixelData = &pixels[0];
			IDirectDrawSurface4* tmpSurface = AllocateTemporaryTexture(width, height);

			DDSURFACEDESC2 desc;
			desc.dwSize = sizeof(desc);

			DDSCAPS2 caps;
			memset(&caps, 0, sizeof(caps));
			caps.dwCaps = DDSCAPS_TEXTURE | DDSCAPS_MIPMAP;

			IDirectDrawSurface4* mipSurf = surface;
			int level = 0;
			HRESULT res = DD_OK;

			while (res == DD_OK)
			{
				DDSURFACEDESC2 desc;
				desc.dwSize = sizeof(desc);
				Guard(mipSurf->GetSurfaceDesc(&desc));

				if (desc.dwWidth == width && desc.dwHeight == height)
				{
					Guard(tmpSurface->Lock(0, &desc, DDLOCK_WRITEONLY | DDLOCK_WAIT, 0));
					memcpy(desc.lpSurface, pixelData, pixels->Length); // Warning, this can potentially crash application if method supplied with non-full bitmap buffer
					Guard(tmpSurface->Unlock(0));

					Console::WriteLine("Uploading mip {0}x{1}", desc.dwWidth, desc.dwHeight);

					Guard(mipSurf->Blt(0, tmpSurface, 0, DDBLT_WAIT, 0));
					tmpSurface->Release();

					mipSurf->Release();
					return;
				}

				IDirectDrawSurface4* currSurf = mipSurf;
				res = mipSurf->GetAttachedSurface(&caps, &mipSurf);
				//currSurf->Release();
			}
			
			throw gcnew ArgumentException("Bug in mipmap uploading code (requested mip not found)");
		}

		void Texture::FromHBitmap(IntPtr hbitmap)
		{
			if (!hbitmap.ToPointer())
				throw gcnew ArgumentException("hbitmap can't be null");

			HBITMAP bmp = (HBITMAP)hbitmap.ToPointer();
			tagBITMAP bmpDesc;
			GetObject(bmp, sizeof(bmpDesc), &bmpDesc);

			IDirectDrawSurface4* tmpSurface = AllocateTemporaryTexture(Width, Height);
			HDC dc = CreateCompatibleDC(0);
			SelectObject(dc, bmp);
			HDC tmpDc;
			Guard(tmpSurface->GetDC(&tmpDc));

			BitBlt(tmpDc, 0, 0, bmpDesc.bmWidth, bmpDesc.bmHeight, dc, 0, 0, SRCCOPY);
			tmpSurface->ReleaseDC(tmpDc);
			Guard(surface->Blt(0, tmpSurface, 0, DDBLT_WAIT, 0));

			tmpSurface->Release();
		}
	}
}