using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Tao.DevIl;
using Tao.FreeGlut;
using Tao.OpenGl;
using Tao.Platform.Windows;

namespace Terminator
{
    public partial class SceneForm : Form
    {
        float[] _lightPos = { 0f, 23f, 70f, 1.0f };
        float[] _planeNormal = { 0.0f, 1.0f, 0.0f, 0.0f };
        private uint asd;

        public SceneForm()
        {
            InitializeComponent();
            AnT.InitializeContexts();
            MouseWheel += AnT_MouseWheel;
        }

        double a = 0, b = 0, c = -3, d = 0, zoom = 0.1, delta = 0;
        int os_x = 1, os_y = 0, os_z = 0;
        bool Wire = false;


        private float angleArm = 0, stepAngleArm = 1, angleForearmLeft, angleForearmRight, angleForearm2 = 0, step2 = 0.28f;
        private bool isLeftArm = true;


        ModelLoader Model = null;

        private void Form1_Load(object sender, EventArgs e)
        {
            Glut.glutInit();
            Glut.glutInitDisplayMode(Glut.GLUT_RGB | Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH);
            Il.ilInit();

            Il.ilEnable(Il.IL_ORIGIN_SET);


            Gl.glClearColor(0, 0, 0, 1);

            Gl.glViewport(0, 0, AnT.Width, AnT.Height);

            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();

            Glu.gluPerspective(45, (float)AnT.Width / (float)AnT.Height, 0.1, 200);

            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();

            Gl.glEnable(Gl.GL_DEPTH_TEST);
            Gl.glEnable(Gl.GL_LIGHTING);
            Gl.glEnable(Gl.GL_LIGHT0);

            float[] ambience = { 0.3f, 0.3f, 0.3f, 1.0f };    // Цвет мирового света
            float[] diffuse = { 0.5f, 0.5f, 0.5f, 1.0f }; // Цвет позиционного света

            // Чтобы установить мировое освещене, нужно передать OpenGL наши массивы.
            // OpenGL даёт нам возможность использовать несколько источников света. Общее количество
            // источников зависит от переменной GL_MAX_LIGHTS. Для первого используемого источника будем
            // использовать дефайн OpenGL GL_LIGHT0. После указания используемого источника передаём 
            // OpenGL флаг говорящий, что мы устанавливаем значение ambience, и массив ambience-цветов.
            // Точно то же делаем с diffuse и GL_DIFFUSE.

            // Устанавливаем цвет рассеянного цвета (без направленного света)
            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_AMBIENT, ambience);
            // И диффузный цвет (цвет света)
            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_DIFFUSE, diffuse);

            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_POSITION, _lightPos);

            Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
            Gl.glEnable(Gl.GL_BLEND);
            Gl.glEnable(Gl.GL_LINE_SMOOTH);
            Gl.glLineWidth(1.0f);

            Gl.glEnable(Gl.GL_COLOR_MATERIAL);


            //LoadModel(@"d:\Учёба\МГ\textures\tanks_carrier.ase");
            //LoadModel(@"d:\sunf.ASE");

            var texture = new TexturesForObjects();
            texture.LoadTextureForModel(@"d:\grass.jpg");
            asd = texture.GetTextureObj();
            RenderTimer.Start();
        }

        private void LoadModel(string pathToModel)
        {
            Model = new ModelLoader();
            Model.LoadModel(pathToModel);
        }

        void DrawCube(float width, float height, float thickness, float x, float y, float z)
        {
            // Передний полигон
            Gl.glBegin(Gl.GL_QUADS);
            Gl.glVertex3f(-0.2f, 0.0f, 0.0f);
            Gl.glVertex3f(-0.2f, 0.5f, 0.0f);
            Gl.glVertex3f(0.3f, 0.5f, 0.0f);
            Gl.glVertex3f(0.3f, 0.0f, 0.0f);
            Gl.glEnd();

            // Задний полигон
            Gl.glBegin(Gl.GL_QUADS);
            Gl.glVertex3f(-0.2f, 0.0f, -0.5f);
            Gl.glVertex3f(-0.2f, 0.5f, -0.5f);
            Gl.glVertex3f(0.3f, 0.5f, -0.5f);
            Gl.glVertex3f(0.3f, 0.0f, -0.5f);
            Gl.glEnd();

            // Левый полигон
            Gl.glBegin(Gl.GL_QUADS);
            Gl.glVertex3f(-0.2f, 0.0f, -0.5f);
            Gl.glVertex3f(-0.2f, 0.5f, -0.5f);
            Gl.glVertex3f(-0.2f, 0.5f, 0.0f);
            Gl.glVertex3f(-0.2f, 0.0f, 0.0f);
            Gl.glEnd();

            // Правый полигон
            Gl.glBegin(Gl.GL_QUADS);
            Gl.glVertex3f(0.3f, 0.0f, -0.5f);
            Gl.glVertex3f(0.3f, 0.5f, -0.5f);
            Gl.glVertex3f(0.3f, 0.5f, 0.0f);
            Gl.glVertex3f(0.3f, 0.0f, 0.0f);
            Gl.glEnd();

            // Верхний полигон
            Gl.glBegin(Gl.GL_QUADS);
            Gl.glVertex3f(-0.2f, 0.5f, 0.0f);
            Gl.glVertex3f(-0.2f, 0.5f, -0.5f);
            Gl.glVertex3f(0.3f, 0.5f, -0.5f);
            Gl.glVertex3f(0.3f, 0.5f, 0.0f);
            Gl.glEnd();

            // Нижний полигон
            Gl.glBegin(Gl.GL_QUADS);
            Gl.glVertex3f(-0.2f, 0.0f, 0.0f);
            Gl.glVertex3f(-0.2f, 0.0f, -0.5f);
            Gl.glVertex3f(0.3f, 0.0f, -0.5f);
            Gl.glVertex3f(0.3f, 0.0f, 0.0f);
            Gl.glEnd();
        }

        public void DrawHead()
        {
            //head
            Glut.glutWireSphere(0.5f, 10, 10);
            Gl.glTranslatef(0.15f, -0.4f, 0);
            Glut.glutWireCube(0.6);

            //neck
            Gl.glPushMatrix();
            Gl.glTranslatef(-0.35f, 0.1f, 0);
            Gl.glRotatef(90f, 1, 0, 0);
            Glut.glutWireCylinder(0.2f, 0.7f, 12, 0);
            Gl.glPopMatrix();

            //body

            //chest
            Gl.glTranslatef(-0.3f, -1f, 0);
            Gl.glPushMatrix();
            Gl.glScaled(0.9, 0.7, 2);
            Gl.glRotatef(-90f, 1, 0, 0);
            Glut.glutWireCube(1.0);
            Gl.glPopMatrix();

            //Torso
            Gl.glPushMatrix();
            Gl.glTranslatef(0.47f, -0.3f, 1);
            Gl.glRotatef(90f, 0, 1, 0);
            Gl.glScaled(2, 2.9, 1.56);
            DrawWireTorso();
            Gl.glPopMatrix();

            //tail
            Gl.glPushMatrix();
            Gl.glTranslatef(-0.1f, -1f, 0f);
            Gl.glRotatef(90f, 1, 0, 0);
            Glut.glutWireCylinder(0.2f, 1f, 10, 0);
            Gl.glPopMatrix();

            Gl.glPushMatrix();
            Gl.glTranslatef(0f, -2f, 0f);
            Gl.glRotatef(100f, 1, 0, 0);
            Glut.glutWireCylinder(0.05f, 0.4f, 10, 0);
            Gl.glPopMatrix();

            Gl.glPushMatrix();
            Gl.glTranslatef(-0.1f, -2f, 0f);
            Gl.glRotatef(80f, 1, 0, 0);
            Glut.glutWireCylinder(0.05f, 0.4f, 10, 0);
            Gl.glPopMatrix();

            Gl.glPushMatrix();
            Gl.glTranslatef(-0.1f, -2f, 0.1f);
            Gl.glRotatef(77f, 1, 0, 0);
            Glut.glutWireCylinder(0.05f, 0.4f, 10, 0);
            Gl.glPopMatrix();

            //left shoulder
            Gl.glPushMatrix();
            Gl.glTranslatef(0f, 0f, -1.3f);
            Glut.glutWireCylinder(0.15f, 0.3f, 25, 0);
            Gl.glPopMatrix();

            Gl.glPushMatrix();

            //left forearm

            Gl.glPushMatrix();
            Gl.glTranslatef(0f, 0f, -1.15f);
            Gl.glRotatef(-30, 0, 1, 0);
            Gl.glPushMatrix();
            Gl.glRotatef(-160f + angleForearmLeft , 1, 0, 0);
            Glut.glutWireCylinder(0.15f, 1.3f, 10, 0);
            Gl.glPopMatrix();
            Gl.glPopMatrix();

            var deX = 1.3 * Math.Sin(ConvertToRadians(30)) * Math.Cos(ConvertToRadians(20 + angleForearmLeft));
            var deY = 1.3*Math.Sin(ConvertToRadians(20 + angleForearmLeft)) + 0.1f;
            var deZ = 1.3 * Math.Cos(ConvertToRadians(30)) * Math.Cos(ConvertToRadians(20 + angleForearmLeft));

            Gl.glTranslatef((float)deX, (float)deY, -1.15f - (float)deZ);

            //left elbow
            Gl.glPushMatrix();
            Gl.glTranslatef(0f, -0.1f, -0.1f);
            Glut.glutWireCylinder(0.15f, 0.3f, 10, 0);
            Gl.glPopMatrix();

            Gl.glTranslatef(0f, -0.15f, 0f);

            //left arm
            Gl.glPushMatrix();
            //Gl.glTranslatef(0f, -1.3f, -1.15f);
            Gl.glRotatef(-20f, 1, 0, 0);
            Gl.glRotatef(10f, 0, 1, 0);
            Glut.glutWireCylinder(0.15f, 1.3f, 10, 0);
            Gl.glPopMatrix();

            Gl.glTranslatef(0.3f, 0.4f, 1.35f);

            //left fingers
            Gl.glPushMatrix();
            Gl.glRotatef(72f, 1, 0, 0);
            Gl.glRotatef(0f, 0, 1, 0);
            Gl.glRotatef(-40f, 0, 0, 1);
            Gl.glScaled(0.04, 0.5, 0.04);
            Glut.glutWireCube(1.0);
            Gl.glTranslatef(0f, 0f, -1.12f);
            Glut.glutWireCube(1.0);
            Gl.glTranslatef(0f, 0f, -1.19f);
            Glut.glutWireCube(1.0);
            Gl.glTranslatef(0f, 0f, -1.26f);
            Glut.glutWireCube(1.0);
            Gl.glTranslatef(0f, 0f, -1.33f);
            Glut.glutWireCube(1.0);
            Gl.glPopMatrix();

            Gl.glPopMatrix();

            //right shoulder
            Gl.glPushMatrix();
            Gl.glTranslatef(0f, 0f, 1f);
            Glut.glutWireCylinder(0.15f, 0.3f, 25, 0);
            Gl.glPopMatrix();

            Gl.glPushMatrix();

            //right forearm
            Gl.glPushMatrix();
            Gl.glTranslatef(0f, 0f, 1.15f);
            Gl.glRotatef(30, 0, 1, 0);
            Gl.glPushMatrix();
            Gl.glRotatef(-20f - angleForearmRight, 1, 0, 0);
            Glut.glutWireCylinder(0.15f, 1.3f, 10, 0);
            Gl.glPopMatrix();
            Gl.glPopMatrix();

            var deXright = 1.3 * Math.Sin(ConvertToRadians(30)) * Math.Cos(ConvertToRadians(20 + angleForearmRight));
            var deYright = 1.3 * Math.Sin(ConvertToRadians(20 + angleForearmRight)) + 0.1f;
            var deZright = 1.3 * Math.Cos(ConvertToRadians(30)) * Math.Cos(ConvertToRadians(20 + angleForearmRight));

            Gl.glTranslatef((float)deXright, (float)deYright, 1f + (float)deZright);

            //right elbow
            Gl.glPushMatrix();
            Gl.glTranslatef(0f, -0.1f, -0.1f);
            Glut.glutWireCylinder(0.15f, 0.3f, 10, 0);
            Gl.glPopMatrix();

            Gl.glTranslatef(0f, -0.15f, 0.1f);

            //right arm
            Gl.glPushMatrix();
            Gl.glRotatef(20f, 1, 0, 0);
            Gl.glRotatef(170f, 0, 1, 0);
            Glut.glutWireCylinder(0.15f, 1.3f, 10, 0);
            Gl.glPopMatrix();

            Gl.glTranslatef(0.3f, 0.4f, -1.3f);
            //right fingers
            Gl.glPushMatrix();
            Gl.glRotatef(-78f, 1, 0, 0);
            Gl.glRotatef(0f, 0, 1, 0);
            Gl.glRotatef(-40f, 0, 0, 1);
            Gl.glScaled(0.04, 0.5, 0.04);
            Glut.glutWireCube(1.0);
            Gl.glTranslatef(0f, 0f, 1.12f);
            Glut.glutWireCube(1.0);
            Gl.glTranslatef(0f, 0f, 1.19f);
            Glut.glutWireCube(1.0);
            Gl.glTranslatef(0f, 0f, 1.26f);
            Glut.glutWireCube(1.0);
            Gl.glTranslatef(0f, 0f, 1.33f);
            Glut.glutWireCube(1.0);
            Gl.glPopMatrix();

            Gl.glPopMatrix();
        }

        private void DrawSolidTorso()
        {
            Gl.glBegin(Gl.GL_QUADS);
            Gl.glVertex3f(0, 0, 0);
            Gl.glVertex3f(1, 0, 0);
            Gl.glVertex3f(0.8f, -0.4f, -0.2f);
            Gl.glVertex3f(0.2f, -0.4f, -0.2f);

            Gl.glVertex3f(0, 0, 0);
            Gl.glVertex3f(0, 0, -0.6f);
            Gl.glVertex3f(1f, 0, -0.6f);
            Gl.glVertex3f(1, 0, 0);

            Gl.glVertex3f(0, 0, -0.6f);
            Gl.glVertex3f(1, 0, -0.6f);
            Gl.glVertex3f(0.8f, -0.4f, -0.5f);
            Gl.glVertex3f(0.2f, -0.4f, -0.5f);

            Gl.glVertex3f(0.2f, -0.4f, -0.5f);
            Gl.glVertex3f(0.8f, -0.4f, -0.5f);
            Gl.glVertex3f(0.8f, -0.4f, -0.2f);
            Gl.glVertex3f(0.2f, -0.4f, -0.2f);

            Gl.glVertex3f(0, 0, 0);
            Gl.glVertex3f(0, 0, -0.6f);
            Gl.glVertex3f(0.2f, -0.4f, -0.5f);
            Gl.glVertex3f(0.2f, -0.4f, -0.2f);

            Gl.glVertex3f(1, 0, 0);
            Gl.glVertex3f(1, 0, -0.6f);
            Gl.glVertex3f(0.8f, -0.4f, -0.5f);
            Gl.glVertex3f(0.8f, -0.4f, -0.2f);

            Gl.glEnd();
        }

        private void DrawWireTorso()
        {
            Gl.glBegin(Gl.GL_LINES);

            Gl.glVertex3f(0, 0, 0);
            Gl.glVertex3f(1, 0, 0);
            Gl.glVertex3f(1, 0, 0);
            Gl.glVertex3f(0.8f, -0.4f, -0.2f);
            Gl.glVertex3f(0.8f, -0.4f, -0.2f);
            Gl.glVertex3f(0.2f, -0.4f, -0.2f);
            Gl.glVertex3f(0.2f, -0.4f, -0.2f);
            Gl.glVertex3f(0, 0, 0);

            Gl.glVertex3f(0, 0, 0);
            Gl.glVertex3f(0, 0, -0.6f);
            Gl.glVertex3f(0, 0, -0.6f);
            Gl.glVertex3f(1f, 0, -0.6f);
            Gl.glVertex3f(1f, 0, -0.6f);
            Gl.glVertex3f(1, 0, 0);
            Gl.glVertex3f(1, 0, 0);
            Gl.glVertex3f(0, 0, 0);

            Gl.glVertex3f(0, 0, -0.6f);
            Gl.glVertex3f(1, 0, -0.6f);
            Gl.glVertex3f(1, 0, -0.6f);
            Gl.glVertex3f(0.8f, -0.4f, -0.5f);
            Gl.glVertex3f(0.8f, -0.4f, -0.5f);
            Gl.glVertex3f(0.2f, -0.4f, -0.5f);
            Gl.glVertex3f(0.2f, -0.4f, -0.5f);
            Gl.glVertex3f(0, 0, -0.6f);

            Gl.glVertex3f(0.2f, -0.4f, -0.5f);
            Gl.glVertex3f(0.8f, -0.4f, -0.5f);
            Gl.glVertex3f(0.8f, -0.4f, -0.5f);
            Gl.glVertex3f(0.8f, -0.4f, -0.2f);
            Gl.glVertex3f(0.8f, -0.4f, -0.2f);
            Gl.glVertex3f(0.2f, -0.4f, -0.2f);
            Gl.glVertex3f(0.2f, -0.4f, -0.2f);
            Gl.glVertex3f(0.2f, -0.4f, -0.5f);

            Gl.glVertex3f(0, 0, 0);
            Gl.glVertex3f(0, 0, -0.6f);
            Gl.glVertex3f(0, 0, -0.6f);
            Gl.glVertex3f(0.2f, -0.4f, -0.5f);
            Gl.glVertex3f(0.2f, -0.4f, -0.5f);
            Gl.glVertex3f(0.2f, -0.4f, -0.2f);
            Gl.glVertex3f(0.2f, -0.4f, -0.2f);
            Gl.glVertex3f(0, 0, 0);

            Gl.glVertex3f(1, 0, 0);
            Gl.glVertex3f(1, 0, -0.6f);
            Gl.glVertex3f(1, 0, -0.6f);
            Gl.glVertex3f(0.8f, -0.4f, -0.5f);
            Gl.glVertex3f(0.8f, -0.4f, -0.5f);
            Gl.glVertex3f(0.8f, -0.4f, -0.2f);
            Gl.glVertex3f(0.8f, -0.4f, -0.2f);
            Gl.glVertex3f(1, 0, 0);

            Gl.glEnd();
        }

        private void Draw()
        {
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);

            Gl.glLoadIdentity();
            Gl.glColor3i(255, 0, 0);

            Gl.glTranslated(a, b, c);
            Gl.glRotated(angleX, 0, 1, 0);
            Gl.glRotated(angleY, 1, 0, 0);
            Gl.glScaled(zoom, zoom, zoom);
            Gl.glEnable(Gl.GL_TEXTURE_2D);
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, asd);
            Gl.glBegin(Gl.GL_QUADS);
            Gl.glTexCoord2d(0.0, 0.0); Gl.glVertex3f(15.0f, -0.01f, 15.0f);
            Gl.glTexCoord2d(1.0, 0.0); Gl.glVertex3f(-15.0f, -0.01f, 15.0f);
            Gl.glTexCoord2d(1.0, 1.0); Gl.glVertex3f(-15.0f, -0.01f, -15.0f);
            Gl.glTexCoord2d(0.0, 1.0); Gl.glVertex3f(15.0f, -0.01f, -15.0f);
            Gl.glEnd();
            Gl.glDisable(Gl.GL_TEXTURE_2D);

            //if (Model != null)
            //    Model.DrawModel();

            var shadowMatrix = CreateShadowMatrix(_planeNormal, _lightPos);

            Gl.glDisable(Gl.GL_DEPTH_TEST);

            Gl.glPushMatrix();
            Gl.glMultMatrixf(shadowMatrix);
            Gl.glColor3f(0, 0, 0);
            Gl.glTranslated(0, 2, 0);
            //Gl.glRotated(-70, 0, 0, 1);
            DrawHead();
            Gl.glPopMatrix();

            Gl.glColor3f(255f, 0f, 0f);
            Gl.glTranslated(0, 2, 0);
            //Gl.glRotated(-70, 0, 0, 1);
            DrawHead();

            Gl.glFlush();

            AnT.Invalidate();
        }

        void DrawGround()
        {
            float fExtent = 20f, fStep = 1f, y = -0.0f;
            float iLine;

            //Gl.glBegin(Gl.GL_LINES);
            //for (iLine = -fExtent; iLine < fExtent; iLine += fStep)
            //{
            //    Gl.glVertex3f(iLine, fExtent, y);
            //    Gl.glVertex3f(iLine, -fExtent, y);
            //    Gl.glVertex3f(fExtent, iLine, y);
            //    Gl.glVertex3f(-fExtent, iLine, y);
            //}
            //Gl.glEnd();

            Gl.glColor3i(255, 255, 0);

            Gl.glBegin(Gl.GL_QUADS);
            Gl.glVertex3f(fExtent, fExtent, y);
            Gl.glVertex3f(-fExtent, fExtent, y);
            Gl.glVertex3f(-fExtent, -fExtent, y);
            Gl.glVertex3f(fExtent, -fExtent, y);
            Gl.glEnd();
        }

        private void RenderTimer_Tick(object sender, EventArgs e)
        {
            Draw();

            if (angleForearmLeft > 30 || angleForearmRight> 30)
                stepAngleArm = -stepAngleArm;

            if (angleForearmRight < 0 && !isLeftArm)
            {
                stepAngleArm = -stepAngleArm;
                angleForearmLeft = 0;
                isLeftArm = true;
            }

            if (angleForearmLeft < 0 && isLeftArm)
            {
                stepAngleArm = -stepAngleArm;
                angleForearmRight = 0;
                isLeftArm = false;
            }
            if (isLeftArm)
                angleForearmLeft += stepAngleArm;
            else angleForearmRight += stepAngleArm;

            label11.Text = angleForearmLeft.ToString();
        }

        private void AnT_MouseWheel(object sender, MouseEventArgs e)
        {
            //label12.Text = zoom.ToString();
            if (e.Delta < 0 && zoom > 0)
                zoom -= 0.02f;
            else zoom += 0.02f;
            //Draw();
        }

        private void AnT_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                label10.Text = "mousemove";
                angleX = e.X;// - prev_x;
                angleY = e.Y;// - prev_y;
            }
        }

        private double prev_x = 0, prev_y = 0, angleX, angleY;

        private void AnT_MouseDown(object sender, MouseEventArgs e)
        {
            prev_x = e.X;
            prev_y = e.Y;
            //label11.Text = "X = " + prev_x + " Y = " + prev_y;
        }

        private void AnT_MouseUp(object sender, MouseEventArgs e)
        {

            // label11.Text = "X = " + prev_x + " Y = " + prev_y;
        }

        private float[] CreateShadowMatrix(float[] planeNormal, float[] lightPos)
        {
            // Чтобы создать матрицу теней, сначала нужно получить скалярное произведение нормали
            // поверхности и позиции света. Мы сохраним результат в float-переменной dotProduct.
            // Используем функцию DotProduct из нашего класса CVector4.
            float dotProduct = planeNormal[0] * lightPos[0] + planeNormal[1] * lightPos[1] + planeNormal[2] * lightPos[2] + planeNormal[3] * lightPos[3];
            float[] matrix = new float[16];

            // Создаем матрицу теней путем добавления наших значений...
            matrix[0] = dotProduct - lightPos[0] * planeNormal[0];
            matrix[4] = 0.0f - lightPos[0] * planeNormal[1];
            matrix[8] = 0.0f - lightPos[0] * planeNormal[2];
            matrix[12] = 0.0f - lightPos[0] * planeNormal[3];

            matrix[1] = 0.0f - lightPos[1] * planeNormal[0];
            matrix[5] = dotProduct - lightPos[1] * planeNormal[1];
            matrix[9] = 0.0f - lightPos[1] * planeNormal[2];
            matrix[13] = 0.0f - lightPos[1] * planeNormal[3];

            matrix[2] = 0.0f - lightPos[2] * planeNormal[0];
            matrix[6] = 0.0f - lightPos[2] * planeNormal[1];
            matrix[10] = dotProduct - lightPos[2] * planeNormal[2];
            matrix[14] = 0.0f - lightPos[2] * planeNormal[3];

            matrix[3] = 0.0f - lightPos[3] * planeNormal[0];
            matrix[7] = 0.0f - lightPos[3] * planeNormal[1];
            matrix[11] = 0.0f - lightPos[3] * planeNormal[2];
            matrix[15] = dotProduct - lightPos[3] * planeNormal[3];

            return matrix;
        }

        public double ConvertToRadians(double angle)
        {
            return (Math.PI / 180) * angle;
        }
    }
}
