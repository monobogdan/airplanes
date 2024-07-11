using System;
using System.Collections.Generic;
using System.Text;
using DXSharp.D3D;

namespace Planes3D
{
    public sealed class Camera
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public float FOV;
        public float Aspect;
        public float Near;
        public float Far;

        public Matrix View;
        public Matrix Projection;

        public Frustum Frustum;

        public Camera()
        {
            FOV = 60;
            Aspect = (float)4.0f / 3.0f;
            Near = 0.1f;
            Far = 300;

            Frustum = new Frustum();

            MarkUpdated();
        }

        public bool IsAABBVisible(Vector3 position, BoundingBox bbox)
        {
            bbox.X += position.X;
            bbox.Y += position.Y;
            bbox.Z += position.Z;
            bbox.X2 += bbox.X;
            bbox.Y2 += bbox.Y;
            bbox.Z2 += bbox.Z;

            // We take nearest up and fartest down point since it's a bit faster than comparing all four points
            return Frustum.IsPointInFrustum(bbox.X, bbox.Y, bbox.Z) &&
                Frustum.IsPointInFrustum(bbox.X2, bbox.Y2, bbox.Z2);
        }

        public bool IsSphereVisible(Vector3 position, float radius)
        {
            return Frustum.IsSphereInFrustum(position.X, position.Y, position.Z, radius);
        }

        public void MarkUpdated()
        {
            View = Matrix.RotationZ(-Rotation.Z * MathUtils.DegToRad) *
                               Matrix.RotationX(-Rotation.X * MathUtils.DegToRad) *
                               Matrix.RotationY(-Rotation.Y * MathUtils.DegToRad) *
                               Matrix.Translation(-Position.X, -Position.Y, -Position.Z);
            Projection = Matrix.Perspective(FOV * MathUtils.DegToRad, Aspect, Near, Far);

            Frustum.Calculate(Projection * View);
        }
    }

    public sealed class GraphicsStats
    {
        public int NumDrawCalls;
        public int NumTriangles;
        public int TextureMemoryPressure;
        public float FrameTime;

        private float NextUpdate;

        public void Update()
        {
            if (NextUpdate < 0)
            {
                Log.WriteLine("DrawCalls: {0}, Triangle count: {1}, TextureMemoryPressure: {2}, Frame time: {3}", NumDrawCalls, NumTriangles, TextureMemoryPressure, FrameTime);

                NextUpdate = 1;
            }

            NextUpdate -= 0.01f;
        }
    }

    public sealed class Graphics
    {
        public Device Context;
        public Camera Camera;

        public Skybox Sky;

        private List<Light> lights;

        public GraphicsStats Stats;

        private DXSharp.D3D.Material materialDesc;

        internal Graphics()
        {
            // Initialize context
            Context = Engine.Current.Window.CreateDevice();
            Context.AttachViewport(Engine.Current.Window.Width, Engine.Current.Window.Height, -1, 2, 1, 2, 1);

            Sky = new Skybox();
            Sky.Load("miramar");

            lights = new List<Light>();

            Camera = new Camera();
            materialDesc = new DXSharp.D3D.Material();

            Stats = new GraphicsStats();
        }

        public void AddLight(Light light)
        {
            if (!lights.Contains(light))
            {
                lights.Add(light);

                Context.AddLight(light);
            }
        }

        public void RemoveLight(Light light)
        {
            if (lights.Contains(light))
            {
                lights.Remove(light);

                Context.RemoveLight(light);
            }
        }

        public void BeginScene()
        {
            Stats.NumDrawCalls = 0;
            Stats.NumTriangles = 0;

            // Prepare view and projection matrices
            Context.SetTransform(TransformType.View, Camera.View.Items);
            Context.SetTransform(TransformType.Projection, Camera.Projection.Items);

            Context.Clear(1, new Rect() { X = 0, Y = 0, Width = Engine.Current.Window.Width, Height = Engine.Current.Window.Height }, ClearTarget.Target, new Color(0, 0, 255, 255), 1.0f, 0);
            Context.Clear(1, new Rect() { X = 0, Y = 0, Width = Engine.Current.Window.Width, Height = Engine.Current.Window.Height }, ClearTarget.ZBuffer, new Color(0, 0, 255, 255), 1.0f, 0);
            Context.BeginScene();

            Sky.Draw();
        }

        public void EndScene()
        {
            Context.EndScene();

            Stats.Update();
        }

        private void ResetCombinerState()
        {
            Context.SetTextureStageState(0, (int)TextureStageState.ColorOp, (int)TextureStageOp.Modulate);
            Context.SetTextureStageState(0, (int)TextureStageState.ColorArg1, (int)TextureArgument.Diffuse);
            Context.SetTextureStageState(0, (int)TextureStageState.ColorArg2, (int)TextureArgument.Texture);
            Context.SetTextureStageState(0, (int)TextureStageState.AlphaOp, (int)TextureStageOp.Modulate);
            Context.SetTextureStageState(0, (int)TextureStageState.AlphaArg1, (int)TextureArgument.Diffuse);
            Context.SetTextureStageState(0, (int)TextureStageState.AlphaArg2, (int)TextureArgument.Texture);
            Context.SetTextureStageState(1, (int)TextureStageState.ColorOp, (int)TextureStageOp.Disable);
            Context.SetTextureStageState(1, (int)TextureStageState.AlphaOp, (int)TextureStageOp.Disable);
        }

        private void CombinerTerrainEffect(Material mat)
        {
            if (mat.Detail != null)
            {
                Context.SetRenderState(RenderState.TFactor, new Color(255, 255, 255, 255).GetRGBA());

                Context.SetTextureStageState(1, (int)TextureStageState.AlphaOp, (int)TextureStageOp.Modulate);
                Context.SetTextureStageState(1, (int)TextureStageState.AlphaArg1, (int)TextureArgument.Texture);
                Context.SetTextureStageState(1, (int)TextureStageState.AlphaArg2, (int)TextureArgument.Texture);

                Context.SetTextureStageState(0, (int)TextureStageState.ColorOp, (int)TextureStageOp.SelectArg1);
                Context.SetTextureStageState(0, (int)TextureStageState.ColorArg1, (int)TextureArgument.Texture);
                Context.SetTextureStageState(0, (int)TextureStageState.ColorArg2, (int)TextureArgument.Texture);

                Context.SetTextureStageState(1, (int)TextureStageState.ColorOp, (int)TextureStageOp.BlendDiffuseAlpha);
                Context.SetTextureStageState(1, (int)TextureStageState.ColorArg1, (int)TextureArgument.Texture);
                Context.SetTextureStageState(1, (int)TextureStageState.ColorArg2, (int)TextureArgument.Current);

                

                Context.SetTexture(1, mat.Detail);
            }
        }

        private void SetTextureStage(Material material)
        {
            /* Create D3D material description from engine material */
            materialDesc.DiffuseR = material.Diffuse.X;
            materialDesc.DiffuseG = material.Diffuse.Y;
            materialDesc.DiffuseB = material.Diffuse.Z;
            materialDesc.DiffuseA = material.Diffuse.W;

            materialDesc.AmbientR = 0.2f;
            materialDesc.AmbientG = 0.2f;
            materialDesc.AmbientB = 0.2f;

            materialDesc.SpecularR = material.Specular.X;
            materialDesc.SpecularG = material.Specular.Y;
            materialDesc.SpecularB = material.Specular.Z;

            materialDesc.Power = material.Shininess;

            if (material.Texture != null)
                Context.SetTexture(0, material.Texture);

            Context.SetMaterial(materialDesc);

            ResetCombinerState(); // Default combiner state is default effect, i.e single texture without any combiner-effects

            switch(material.Effect)
            {
                case MaterialEffect.Terrain:
                    CombinerTerrainEffect(material);
                    break;
            }
        }

        public void DrawMesh(Mesh mesh, int startVertex, int endVertex, Vector3 position, Vector3 rotation, Vector3 scaling, Material materialOverride = null)
        {
            if (mesh != null)
            {
                if (mesh.Radius > 0 && !Camera.IsSphereVisible(position, mesh.Radius))
                    return;

                Material mat = mesh.AssignedMaterial;

                if (materialOverride != null)
                    mat = materialOverride;

                Matrix world = Matrix.Translation(position.X, position.Y, position.Z) *
                               Matrix.RotationY(rotation.Y * MathUtils.DegToRad) *
                               Matrix.RotationZ(rotation.Z * MathUtils.DegToRad) *
                               Matrix.RotationX(rotation.X * MathUtils.DegToRad);
                Context.SetTransform(TransformType.World, world.Items);

                PrimitiveType primitive;

                switch (mesh.Topology)
                {
                    case MeshTopology.Lines:
                        primitive = PrimitiveType.LineList;
                        break;
                    case MeshTopology.Points:
                        primitive = PrimitiveType.PointList;
                        break;
                    default:
                        primitive = PrimitiveType.TriangleList;
                        break;
                }

                if (mat != null)
                    SetTextureStage(mat);

                Context.SetRenderState(RenderState.ZEnable, mat.NoZTest ? 0 : 1);

                Context.Begin(primitive, Device.VertexFormat, mat.IsLit);
                for (int i = startVertex; i < endVertex; i++)
                    Context.Vertex(mesh.Vertices[i]);
                Context.End();

                Stats.NumDrawCalls++;
                Stats.NumTriangles += mesh.Vertices.Length / 3;
            }
        }

        public void DrawMesh(Mesh mesh, Vector3 position, Vector3 rotation, Vector3 scaling, Material materialOverride = null)
        {
            DrawMesh(mesh, 0, mesh.Vertices.Length, position, rotation, scaling, materialOverride);
        }
    }
}
