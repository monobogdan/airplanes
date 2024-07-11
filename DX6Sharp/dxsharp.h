#pragma once

#define WIN32_LEAN_AND_MEAN  

#include <Windows.h>
#include <MMReg.h>
#include <d3d.h>
#include <ddraw.h>
#include <dsound.h>

#define Guard(expr) DXSharp::Helpers::ExceptionManager::Assert(expr, #expr)

using namespace System;

namespace DXSharp
{

	namespace D3D
	{
		ref class Device;
	}

	namespace Helpers
	{
		ref class ExceptionManager
		{
		public:
			static void Assert(HRESULT res, String^ methodName);
		};

		public ref class Window
		{
		internal:
			HWND hwnd;
			IDirectDraw* ddraw;
			IDirectDrawSurface4* primarySurface;
			IDirectDrawSurface4* d3dSurface;

			static DDPIXELFORMAT* opaqueTextureFormat; // HACK
			static DDPIXELFORMAT* zBufferFormat;

			void CreateDDrawContext();
		public:
			int Width;
			int Height;

			Window(int width, int height, bool isFullScreen);
			
			IntPtr GetHandle();

			DXSharp::D3D::Device^ CreateDevice();
			bool DoEvents();
			void Present();
		};
	}

	namespace Sound
	{
		public enum class BufferFlags
		{
			ControlDefault = DSBCAPS_CTRLDEFAULT,
			PrimaryBuffer = DSBCAPS_PRIMARYBUFFER,
			Static = DSBCAPS_STATIC,
			Software = DSBCAPS_LOCSOFTWARE,
			Control3D = DSBCAPS_CTRL3D
		};

		public value struct WaveFormat
		{
			WORD        FormatTag;         /* format type */
			WORD        Channels;          /* number of channels (i.e. mono, stereo...) */
			DWORD       SamplesPerSec;     /* sample rate */
			DWORD       AvgBytesPerSec;    /* for buffer estimation */
			WORD        BlockAlign;        /* block size of data */
			WORD        BitsPerSample;     /* number of bits per sample of mono data */
		};

		public value struct BufferDescription
		{
			BufferFlags           Flags;
			DWORD           BufferBytes;
			WaveFormat  Format;
		};

		ref class DirectSound;

		public ref class SoundBuffer
		{
		private:
			IDirectSoundBuffer* buffer;
			IDirectSound3DBuffer* buffer3D;
			BufferDescription description;

			void* lockPtr; // Internal lock state
			DWORD lockBytes;

		internal:
			SoundBuffer(IDirectSoundBuffer* buffer, BufferDescription desc);
		public:
			IntPtr Lock();

			void Unlock();

			void Play();
			void Stop();
		};
		
		public ref class DirectSound
		{
		private:
			IDirectSound* dsound;
		public:
			DirectSound();
			~DirectSound();

			void Initialize();
			void Compact();
			SoundBuffer^ CreateSoundBuffer(BufferDescription desc);

			
		};
	}

	namespace D3D
	{
		public enum class ClearTarget
		{
			Target = D3DCLEAR_TARGET,
			ZBuffer = D3DCLEAR_ZBUFFER,
			Stencil = D3DCLEAR_STENCIL
		};

		public enum class PrimitiveType
		{
			PointList = 1,
			LineList = 2,
			LineStrip = 3,
			TriangleList = 4,
			TriangleStrip = 5,
			TriangleFan = 6
		};

		public enum class TransformType
		{
			World = D3DTRANSFORMSTATE_WORLD,
			View = D3DTRANSFORMSTATE_VIEW,
			Projection = D3DTRANSFORMSTATE_PROJECTION
		};

		public ref struct Rect
		{
		public:
			int X;
			int Y;
			int Width;
			int Height;
		};

		public ref struct Color
		{
		public:
			byte R;
			byte G;
			byte B;
			byte A;

			Color(byte r, byte g, byte b, byte a);
			DWORD GetRGBA();
		};

		public ref class Material
		{
		public:
			float DiffuseR, DiffuseG, DiffuseB, DiffuseA;
			float AmbientR, AmbientG, AmbientB;
			float SpecularR, SpecularG, SpecularB;
			float EmissiveR, EmissiveG, EmissiveB;
			float Power;
		};

		public enum class LightType
		{
			Directional = D3DLIGHT_DIRECTIONAL,
			Point = D3DLIGHT_POINT,
			Spot = D3DLIGHT_SPOT
		};
		
		public ref class Light
		{
		internal:
			IDirect3DLight* light;
		public:
			LightType    Type;        /* Type of light source */
			float R, G, B, A;
			float X, Y, Z;
			float DX, DY, DZ; /* Direction */
			float        Range;        /* Cutoff range */
			float        FallOff;      /* Falloff */
			float        LinearAttenuation; /* Linear attenuation */
			float        Theta;        /* Inner angle of spotlight cone */
			float        Phi;          /* Outer angle of spotlight cone */

			Light(Device^ device);
			~Light();

			void Update();
		};

		public enum class RenderState
		{
			ZEnable = D3DRENDERSTATE_ZENABLE,
			ZWriteEnable = D3DRENDERSTATE_ZWRITEENABLE,
			TFactor = D3DRENDERSTATE_TEXTUREFACTOR
		};

		public value struct Vertex
		{
		public:
			float X, Y, Z;
			float NX, NY, NZ;
			D3DCOLOR Diffuse;
			float U, V;
		};

		public enum class TextureStageState
		{
			ColorOp = D3DTSS_COLOROP,
			ColorArg1 = D3DTSS_COLORARG1,
			ColorArg2 = D3DTSS_COLORARG2,
			AlphaOp = D3DTSS_ALPHAOP,
			AlphaArg1 = D3DTSS_ALPHAARG1,
			AlphaArg2 = D3DTSS_ALPHAARG2,
			TexCoordIndex = D3DTSS_TEXCOORDINDEX
		};

		public enum class TextureStageOp
		{
			Disable = D3DTOP_DISABLE,
			Add = D3DTOP_ADD,
			Modulate = D3DTOP_MODULATE,
			Modulate2X = D3DTOP_MODULATE2X,
			Modulate4X = D3DTOP_MODULATE4X,
			SelectArg1 = D3DTOP_SELECTARG1,
			BlendDiffuseAlpha = D3DTOP_BLENDDIFFUSEALPHA
		};

		public enum class TextureArgument
		{
			Current = D3DTA_CURRENT,
			Diffuse = D3DTA_DIFFUSE,
			Texture = D3DTA_TEXTURE,
			TFactor = D3DTA_TFACTOR,
			Complement = D3DTA_COMPLEMENT
		};

		public ref class Texture
		{
			// Details of surface implementation are taken by DXSharp for now
		internal:
			DXSharp::Helpers:: Window^ window;
			
			IDirectDrawSurface4* AllocateTemporaryTexture(int width, int height);

			IDirectDrawSurface4* surface;
			IDirect3DTexture2* texture;
		public:
			int Width;
			int Height;
			int MipCount;

			Texture(DXSharp::Helpers::Window^ window, int width, int height, int mipCount);

			void FromHBitmap(IntPtr hbitmap);
			void FromPixelArray(array<byte>^ pixels, int width, int height, int mipLevel);
		};

		public ref class Device
		{
		private:
			IDirect3DViewport3* currentViewport;
		internal:
			IDirect3D3* direct3d;
			IDirect3DDevice3* device;

			IDirect3DMaterial3* material;

			Device(IDirect3D3* direct3d, IDirect3DDevice3* device);
		public:
			static const int VertexFormat = D3DFVF_XYZ | D3DFVF_NORMAL | D3DFVF_DIFFUSE | D3DFVF_TEX1;

			void AttachViewport(int width, int height, float clipX, float clipWidth, float clipY, float clipHeight, float maxZ);
			void Clear(int count, Rect^ rct, ClearTarget flags, Color^ color, float zValue, int stencilValue);

			void AddLight(Light^ l);
			void RemoveLight(Light^ l);
			
			void BeginScene();
			void EndScene();

			void SetTexture(int stage, Texture^ tex);
			void SetRenderState(RenderState renderState, unsigned int value);
			void SetRenderState(RenderState renderState, float value);
			void SetTextureStageState(int stage, int state, int value);
			void SetMaterial(Material^ material);
			void SetTransform(TransformType transform, array<float>^ matrix);

			void Begin(PrimitiveType primitiveType, int vertexTypeDesc, bool lit);
			void Vertex(Vertex vertex);
			void End();
		};

	}
}