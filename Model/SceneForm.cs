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
        float[] _lightPos = { 0f, 23f, 50f, 1.0f };
        float[] _planeNormal = { 0.0f, 1.0f, 0.0f, 0.0f };
        private uint texIdFace, texIdGround, texIdMaterial, texIdChest, texIdTorso, texIdPress;

        double[] s_coeffs = { 1, 0, 0, 1 }, t_coeffs = { 0, 1, 0, 1 }, r_coeffs = { 1, 0, 1, 0 };

        public SceneForm()
        {
            InitializeComponent();
            AnT.InitializeContexts();
            MouseWheel += AnT_MouseWheel;
        }

        double a = 0, b = 0, c = -3, d = 0, zoom = 0.1, delta = 0;
        int os_x = 1, os_y = 0, os_z = 0;
        bool Wire = false;


        private float angleArm = 0, stepAngleArm = 1, angleForearmLeft, angleForearmRight, move = -3, angleHead = 0, moveHeadZ = 0, moveHeadY = -0.33f, moveHeadX = 0.28f;
        private float moveScapular = 0;
        private double movePress = 0, scaleTerminatorY = 1, scaleTerminatorZ = 1, moveTerminatorY = 0.8, positionLightningX = 0, positionLightningY = 0, positionLightningZ = 0, lightningCount = 0;
        private bool isLeftArm = true, isMove, isAtPlace, isTimeToPress, isLightning, isEyeDown;


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
            Gl.glEnable(Gl.GL_NORMALIZE);
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

            Gl.glEnable(Gl.GL_COLOR_MATERIAL);

            var texture = new TexturesForObjects();
            texture.LoadTextureForModel(@"face.jpg");
            texIdFace = texture.GetTextureObj();
            texture.LoadTextureForModel(@"grass.jpg");
            texIdGround = texture.GetTextureObj();
            texture.LoadTextureForModel(@"material.jpg");
            texIdMaterial = texture.GetTextureObj();
            texture.LoadTextureForModel(@"chest.jpg");
            texIdChest = texture.GetTextureObj();
            texture.LoadTextureForModel(@"torso.jpg");
            texIdTorso = texture.GetTextureObj();
            texture.LoadTextureForModel(@"metal.jpg");
            texIdPress = texture.GetTextureObj();
            RenderTimer.Start();
        }

        public void DrawHead(bool isShadow = false)
        {
            //head
            Gl.glPushMatrix();
            if (!isShadow)
            {
                Gl.glEnable(Gl.GL_TEXTURE_2D);
                Gl.glEnable(Gl.GL_TEXTURE_GEN_S);
                Gl.glEnable(Gl.GL_TEXTURE_GEN_T);
            }
            Gl.glTexGeni(Gl.GL_S, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_SPHERE_MAP);
            Gl.glTexGendv(Gl.GL_S, Gl.GL_EYE_PLANE, s_coeffs);

            Gl.glTexGeni(Gl.GL_T, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_SPHERE_MAP);
            Gl.glTexGendv(Gl.GL_T, Gl.GL_EYE_PLANE, t_coeffs);
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, texIdMaterial);

            Glut.glutSolidSphere(0.5f, 40, 10);

            Gl.glDisable(Gl.GL_TEXTURE_GEN_S);
            Gl.glDisable(Gl.GL_TEXTURE_GEN_T);
            Gl.glDisable(Gl.GL_TEXTURE_2D);


            Gl.glTranslatef(moveHeadX, moveHeadY, moveHeadZ);

            Gl.glPushMatrix();
            Gl.glRotatef(30, 0, 0, 1);
            Gl.glRotatef(angleHead, 0, 1, 0);
            Gl.glScaled(0.09, 0.1, 0.1);
            if (!isShadow)
                DrawCube(0, 0, 0, 7, 7, 7, texIdFace, texIdMaterial);
            else DrawCube(0, 0, 0, 7, 7, 7, 0, 0);
            Gl.glPopMatrix();

            //Eyes

            if (isEyeDown || isShadow)
                Gl.glColor3d(0, 0, 0);
            else
                Gl.glColor3d(1, 0, 0);

            Gl.glPushMatrix();
            Gl.glTranslated(0.2, 0.31, 0.2);
            Glut.glutSolidSphere(0.05f, 10, 10);
            Gl.glPopMatrix();

            Gl.glPushMatrix();
            Gl.glTranslated(0.2, 0.31, -0.2);
            Glut.glutSolidSphere(0.05f, 10, 10);
            Gl.glPopMatrix();

            Gl.glPopMatrix();

            Gl.glTranslatef(0.15f, -0.4f, 0);

            //neck
            Gl.glPushMatrix();
            Gl.glTranslatef(-0.35f, 0.1f, 0);
            Gl.glRotatef(90f, 1, 0, 0);
            if (!isShadow)
            {
                Gl.glEnable(Gl.GL_TEXTURE_2D);
                Gl.glEnable(Gl.GL_TEXTURE_GEN_S);
                Gl.glEnable(Gl.GL_TEXTURE_GEN_T);
            }
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, texIdMaterial);
            Gl.glTexGeni(Gl.GL_S, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_OBJECT_LINEAR);
            Gl.glTexGendv(Gl.GL_S, Gl.GL_OBJECT_PLANE, new double[] { 0, 0, 1, 0 });
            Gl.glTexGendv(Gl.GL_T, Gl.GL_OBJECT_PLANE, new double[] { 0, 0, 1, 0 });


            Glut.glutSolidCylinder(0.2f, 0.8f, 12, 40);
            Gl.glPopMatrix();

            Gl.glDisable(Gl.GL_TEXTURE_GEN_S);
            Gl.glDisable(Gl.GL_TEXTURE_GEN_T);


            //body

            Gl.glPushMatrix();
            Gl.glTranslatef(-0.35f, -0.4f, 0.2f);
            Gl.glRotatef(20f, 1, 0, 0);
            if (!isShadow)
            {
                Gl.glEnable(Gl.GL_TEXTURE_GEN_S);
                Gl.glEnable(Gl.GL_TEXTURE_GEN_T);
            }
            Glut.glutSolidCylinder(0.06f, 0.7f, 12, 10);

            Gl.glPopMatrix();

            Gl.glPushMatrix();
            Gl.glTranslatef(-0.35f, -0.4f, -0.2f);
            Gl.glRotatef(160f, 1, 0, 0);
            Glut.glutSolidCylinder(0.06f, 0.7f, 12, 10);
            Gl.glDisable(Gl.GL_TEXTURE_GEN_S);
            Gl.glDisable(Gl.GL_TEXTURE_GEN_T);

            Gl.glPopMatrix();

            //chest
            Gl.glTranslatef(-0.3f, -1f, 0);
            Gl.glPushMatrix();
            Gl.glScaled(0.1, 0.1, 0.1);
            if (!isShadow)
                DrawCube(0, 0, 0, 9, 7, 20, texIdChest, texIdMaterial);
            else DrawCube(0, 0, 0, 9, 7, 20, 0, 0);
            Gl.glPopMatrix();

            //Torso
            Gl.glPushMatrix();
            Gl.glTranslatef(0.47f, -0.3f, 1);
            Gl.glRotatef(90f, 0, 1, 0);
            Gl.glScaled(0.2, 0.29, 0.156);
            DrawSolidTorso(isShadow);
            Gl.glPopMatrix();

            //tail
            Gl.glPushMatrix();
            if (!isShadow)
            {
                Gl.glEnable(Gl.GL_TEXTURE_2D);
                Gl.glEnable(Gl.GL_TEXTURE_GEN_S);
                Gl.glEnable(Gl.GL_TEXTURE_GEN_T);
            }
            Gl.glTranslatef(-0.1f, -1f, 0f);
            Gl.glRotatef(90f, 1, 0, 0);
            Glut.glutSolidCylinder(0.2f, 1f, 10, 10);
            Gl.glPopMatrix();

            Gl.glPushMatrix();
            Gl.glTranslatef(0f, -2f, 0f);
            Gl.glRotatef(100f, 1, 0, 0);
            Glut.glutSolidCylinder(0.05f, 0.4f, 10, 10);
            Gl.glPopMatrix();

            Gl.glPushMatrix();
            Gl.glTranslatef(-0.1f, -2f, 0f);
            Gl.glRotatef(80f, 1, 0, 0);
            Glut.glutSolidCylinder(0.05f, 0.4f, 10, 10);
            Gl.glPopMatrix();

            Gl.glPushMatrix();
            Gl.glTranslatef(-0.1f, -2f, 0.1f);
            Gl.glRotatef(77f, 1, 0, 0);
            Glut.glutSolidCylinder(0.05f, 0.4f, 10, 10);
            Gl.glPopMatrix();

            //left shoulder
            Gl.glPushMatrix();
            Gl.glTranslatef(0f, 0f, -1.3f);
            Glut.glutSolidCylinder(0.15f, 0.3f, 25, 10);
            Gl.glPopMatrix();

            Gl.glPushMatrix();

            //left forearm

            Gl.glPushMatrix();
            Gl.glTranslatef(0f, 0f, -1.15f);
            Gl.glRotatef(-30, 0, 1, 0);
            Gl.glPushMatrix();
            Gl.glRotatef(-160f + angleForearmLeft, 1, 0, 0);
            Glut.glutSolidCylinder(0.15f, 1.3f, 10, 10);
            Gl.glPopMatrix();
            Gl.glPopMatrix();

            var deX = 1.3 * Math.Sin(ConvertToRadians(30)) * Math.Cos(ConvertToRadians(20 + angleForearmLeft));
            var deY = 1.3 * Math.Sin(ConvertToRadians(20 + angleForearmLeft)) + 0.1f;
            var deZ = 1.3 * Math.Cos(ConvertToRadians(30)) * Math.Cos(ConvertToRadians(20 + angleForearmLeft));

            Gl.glTranslatef((float)deX, (float)deY, -1.15f - (float)deZ);

            //left elbow
            Gl.glPushMatrix();
            Gl.glTranslatef(0f, -0.1f, -0.1f);
            Glut.glutSolidCylinder(0.15f, 0.3f, 10, 10);
            Gl.glPopMatrix();

            Gl.glTranslatef(0f, -0.15f, 0f);

            //left arm
            Gl.glPushMatrix();
            //Gl.glTranslatef(0f, -1.3f, -1.15f);
            Gl.glRotatef(-20f, 1, 0, 0);
            Gl.glRotatef(10f, 0, 1, 0);
            Glut.glutSolidCylinder(0.15f, 1.3f, 10, 10);
            Gl.glPopMatrix();

            if (!isShadow)
            {
                Gl.glDisable(Gl.GL_TEXTURE_GEN_S);
                Gl.glDisable(Gl.GL_TEXTURE_GEN_T);
            }

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

            if (!isShadow)
            {
                Gl.glEnable(Gl.GL_TEXTURE_2D);
                Gl.glEnable(Gl.GL_TEXTURE_GEN_S);
                Gl.glEnable(Gl.GL_TEXTURE_GEN_T);
            }

            //right shoulder
            Gl.glPushMatrix();
            Gl.glTranslatef(0f, 0f, 1f);
            Glut.glutSolidCylinder(0.15f, 0.3f, 25, 10);
            Gl.glPopMatrix();

            Gl.glPushMatrix();

            //right forearm
            Gl.glPushMatrix();
            Gl.glTranslatef(0f, 0f, 1.15f);
            Gl.glRotatef(30, 0, 1, 0);
            Gl.glPushMatrix();
            Gl.glRotatef(-20f - angleForearmRight, 1, 0, 0);
            Glut.glutSolidCylinder(0.15f, 1.3f, 10, 10);
            Gl.glPopMatrix();
            Gl.glPopMatrix();

            var deXright = 1.3 * Math.Sin(ConvertToRadians(30)) * Math.Cos(ConvertToRadians(20 + angleForearmRight));
            var deYright = 1.3 * Math.Sin(ConvertToRadians(20 + angleForearmRight)) + 0.1f;
            var deZright = 1.3 * Math.Cos(ConvertToRadians(30)) * Math.Cos(ConvertToRadians(20 + angleForearmRight));

            Gl.glTranslatef((float)deXright, (float)deYright, 1f + (float)deZright);

            //right elbow
            Gl.glPushMatrix();
            Gl.glTranslatef(0f, -0.1f, -0.1f);
            Glut.glutSolidCylinder(0.15f, 0.3f, 10, 10);
            Gl.glPopMatrix();

            Gl.glTranslatef(0f, -0.15f, 0.1f);

            //right arm
            Gl.glPushMatrix();
            Gl.glRotatef(20f + angleArm, 1, 0, 0);
            Gl.glRotatef(170f, 0, 1, 0);
            Glut.glutSolidCylinder(0.15f, 1.3f, 10, 10);
            Gl.glPopMatrix();

            if (!isShadow)
            {
                Gl.glDisable(Gl.GL_TEXTURE_GEN_S);
                Gl.glDisable(Gl.GL_TEXTURE_GEN_T);
                //Gl.glDisable(Gl.GL_TEXTURE_2D);
            }

            if (isAtPlace)
            {
                deXright = 1.3 * Math.Cos(ConvertToRadians(10)) * Math.Sin(ConvertToRadians(20 + angleArm));
                deYright = 1.3 * Math.Sin(ConvertToRadians(20 + angleForearmRight)) + 0.1f;
                deZright = 1.3 * Math.Cos(ConvertToRadians(10)) * Math.Cos(ConvertToRadians(20 + angleArm)) - 0.1;
                Gl.glTranslatef(0.4f, (float)deXright, (float)-deZright);
            }
            else Gl.glTranslatef(0.3f, 0.4f, -1.3f);
            //right fingers
            Gl.glPushMatrix();


            if (isAtPlace)
            {
                Gl.glTranslatef(-0.2f, 0.1f, 0f);
                Gl.glRotatef(-178f, 1, 0, 0);
            }
            else
            {
                Gl.glRotatef(-78f, 1, 0, 0);
                Gl.glRotatef(0f, 0, 1, 0);
                Gl.glRotatef(-40f, 0, 0, 1);
            }
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

        void DrawCube(float x, float y, float z, float width, float height, float length, uint texIdFront, uint texIdOther)
        {
            x = x - width / 2;
            y = y - height / 2;
            z = z - length / 2;

            if (texIdFront != 0)
            {
                Gl.glEnable(Gl.GL_TEXTURE_2D);
                Gl.glBindTexture(Gl.GL_TEXTURE_2D, texIdOther);
            }

            // забиндим заднюю текстуру на заднюю сторону
            Gl.glBegin(Gl.GL_QUADS);

            // Установим текстурные координаты и вершины ЗАДНЕЙ стороны
            Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(x + width, y, z);
            Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(x + width, y + height, z);
            Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(x, y + height, z);
            Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(x, y, z);

            Gl.glEnd();

            // Начинаем рисовать сторону
            Gl.glBegin(Gl.GL_QUADS);

            // Установим текстурные координаты и вершины ПЕРЕДНЕЙ стороны
            Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(x, y, z + length);
            Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(x, y + height, z + length);
            Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(x + width, y + height, z + length);
            Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(x + width, y, z + length);
            Gl.glEnd();


            // Биндим НИЖНЮЮ текстуру на НИЖНЮЮ сторону бокса

            // Начинаем рисовать сторону
            Gl.glBegin(Gl.GL_QUADS);

            // Установим текстурные координаты и вершины НИЖНЕЙ стороны
            Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(x, y, z);
            Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(x, y, z + length);
            Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(x + width, y, z + length);
            Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(x + width, y, z);
            Gl.glEnd();

            // Биндим ВЕРХНЮЮ текстуру на ВЕРХНЮЮ сторону бокса

            // Начинаем рисовать сторону
            Gl.glBegin(Gl.GL_QUADS);

            // Установим текстурные координаты и вершины ВЕРХНЕЙ стороны
            Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(x + width, y + height, z);
            Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(x + width, y + height, z + length);
            Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(x, y + height, z + length);
            Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(x, y + height, z);

            Gl.glEnd();

            // Биндим ЛЕВУЮ текстуру на ЛЕВУЮ сторону бокса

            // Начинаем рисовать сторону
            Gl.glBegin(Gl.GL_QUADS);

            // Установим текстурные координаты и вершины ЛЕВОЙ стороны
            Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(x, y + height, z);
            Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(x, y + height, z + length);
            Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(x, y, z + length);
            Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(x, y, z);

            Gl.glEnd();

            // Биндим ПРАВУЮ текстуру на ПРАВУЮ сторону бокса

            //Gl.glEnable(Gl.GL_TEXTURE_2D);
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, texIdFront);
            // Начинаем рисовать сторону
            Gl.glBegin(Gl.GL_QUADS);

            // Установим текстурные координаты и вершины ПРАВОЙ стороны
            Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(x + width, y, z);
            Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(x + width, y, z + length);
            Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(x + width, y + height, z + length);
            Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(x + width, y + height, z);
            Gl.glEnd();
            Gl.glDisable(Gl.GL_TEXTURE_2D);
        }

        private void DrawSolidTorso(bool isShadow)
        {
            if (!isShadow)
            {
                Gl.glEnable(Gl.GL_TEXTURE_2D);
                Gl.glBindTexture(Gl.GL_TEXTURE_2D, texIdMaterial);
            }

            Gl.glBegin(Gl.GL_QUADS);
            Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(0, 0, 0);
            Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(0, 0, -6f);
            Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(10f, 0, -6f);
            Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(10, 0, 0);
            Gl.glEnd();

            Gl.glBegin(Gl.GL_QUADS);
            Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(0, 0, -6f);
            Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(10, 0, -6f);
            Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(8f, -4f, -5f);
            Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(2f, -4f, -5f);
            Gl.glEnd();

            Gl.glBegin(Gl.GL_QUADS);
            Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(2f, -4f, -5f);
            Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(8f, -4f, -5f);
            Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(8f, -4f, -2f);
            Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(2f, -4f, -2f);
            Gl.glEnd();

            Gl.glBegin(Gl.GL_QUADS);
            Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(0, 0, 0);
            Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(0, 0, -6f);
            Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(2f, -4f, -5f);
            Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(2f, -4f, -2f);
            Gl.glEnd();

            Gl.glBegin(Gl.GL_QUADS);
            Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(10, 0, 0);
            Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(10, 0, -6f);
            Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(8f, -4f, -5f);
            Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(8f, -4f, -2f);
            Gl.glEnd();

            Gl.glBindTexture(Gl.GL_TEXTURE_2D, texIdTorso);
            Gl.glBegin(Gl.GL_QUADS);
            Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(0, 0, 0);
            Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(10, 0, 0);
            Gl.glTexCoord2f(1f, 0.0f); Gl.glVertex3f(8f, -4f, -2f);
            Gl.glTexCoord2f(0f, 0.0f); Gl.glVertex3f(2f, -4f, -2f);
            Gl.glEnd();

            if (!isShadow)
                Gl.glDisable(Gl.GL_TEXTURE_2D);
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

        private void DrawBarrier()
        {
            Gl.glPushMatrix();
            Gl.glTranslated(7.5, 4, -4.55);
            Gl.glRotated(90, 1, 0, 0);
            for (var i = 0; i < 15; i++)
            {
                Gl.glTranslated(0, 0.55, 0);
                Glut.glutSolidCylinder(0.1, 4, 10, 2);
            }
            Gl.glPopMatrix();
        }

        private void DrawPress(bool isSwadow = false)
        {
            Gl.glPushMatrix();
            if (!isSwadow)
                Gl.glEnable(Gl.GL_TEXTURE_2D);
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, texIdPress);
            Gl.glTranslated(0, 2.7 - movePress, 0);
            DrawCube(0, 0, 0, 15, 0.3f, 7.5f,isSwadow ? 0 : texIdPress, texIdPress);
            Gl.glDisable(Gl.GL_TEXTURE_2D);
            Gl.glPopMatrix();

            Gl.glPushMatrix();
            Gl.glTranslated(2, 5.7, 2);
            Gl.glRotated(90, 1, 0, 0);
            Glut.glutSolidCylinder(0.2, 1.7, 20, 2);
            Gl.glPopMatrix();
            Gl.glPushMatrix();
            Gl.glTranslated(2, 5.7 - movePress, 2);
            Gl.glRotated(90, 1, 0, 0);
            Glut.glutSolidCylinder(0.12, 3, 20, 2);
            Gl.glPopMatrix();

            Gl.glPushMatrix();
            Gl.glTranslated(2, 5.7, -2);
            Gl.glRotated(90, 1, 0, 0);
            Glut.glutSolidCylinder(0.2, 1.7, 20, 2);
            Gl.glPopMatrix();
            Gl.glPushMatrix();
            Gl.glTranslated(2, 5.7 - movePress, -2);
            Gl.glRotated(90, 1, 0, 0);
            Glut.glutSolidCylinder(0.12, 3, 20, 2);
            Gl.glPopMatrix();

            Gl.glPushMatrix();
            Gl.glTranslated(-2, 5.7, -2);
            Gl.glRotated(90, 1, 0, 0);
            Glut.glutSolidCylinder(0.2, 1.7, 20, 2);
            Gl.glPopMatrix();
            Gl.glPushMatrix();
            Gl.glTranslated(-2, 5.7 - movePress, -2);
            Gl.glRotated(90, 1, 0, 0);
            Glut.glutSolidCylinder(0.12, 3, 20, 2);
            Gl.glPopMatrix();

            Gl.glPushMatrix();
            Gl.glTranslated(-2, 5.7, 2);
            Gl.glRotated(90, 1, 0, 0);
            Glut.glutSolidCylinder(0.2, 1.7, 20, 2);
            Gl.glPopMatrix();
            Gl.glPushMatrix();
            Gl.glTranslated(-2, 5.7 - movePress, 2);
            Gl.glRotated(90, 1, 0, 0);
            Glut.glutSolidCylinder(0.12, 3, 20, 2);
            Gl.glPopMatrix();
        }

        private void DrawGround()
        {
            Gl.glEnable(Gl.GL_TEXTURE_2D);
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, texIdPress);
            Gl.glBegin(Gl.GL_QUADS);
            Gl.glTexCoord2d(0.0, 0.0); Gl.glVertex3f(15.0f, 0f, 15.0f);
            Gl.glTexCoord2d(1.0, 0.0); Gl.glVertex3f(-15.0f, 0f, 15.0f);
            Gl.glTexCoord2d(1.0, 1.0); Gl.glVertex3f(-15.0f, 0f, -15.0f);
            Gl.glTexCoord2d(0.0, 1.0); Gl.glVertex3f(15.0f, 0f, -15.0f);
            Gl.glEnd();
            Gl.glDisable(Gl.GL_TEXTURE_2D);
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

            DrawGround();

            var shadowMatrix = CreateShadowMatrix(_planeNormal, _lightPos);

            Gl.glDisable(Gl.GL_DEPTH_TEST);

            Gl.glPushMatrix();
            Gl.glMultMatrixf(shadowMatrix);
            Gl.glColor3f(0, 0, 0);
            DrawBarrier();
            DrawPress(true);
            Gl.glTranslated(move, moveTerminatorY, 0);
            Gl.glScaled(1, scaleTerminatorY, scaleTerminatorZ);
            Gl.glRotated(-80, 0, 0, 1);
            DrawHead(true);
            Gl.glPopMatrix();

            Gl.glEnable(Gl.GL_DEPTH_TEST);

            Gl.glColor3f(0.5f, 0.5f, 0.5f);
            DrawBarrier();
            DrawPress();
            if (isLightning)
                DrawLightning();

            Gl.glTranslated(move, moveTerminatorY, 0);
            Gl.glScaled(1, scaleTerminatorY, scaleTerminatorZ);
            Gl.glRotated(-80, 0, 0, 1);
            DrawHead();
            Gl.glPopMatrix();

            Gl.glFlush();

            AnT.Invalidate();
        }

        private void DrawLightning()
        {
            Gl.glPushMatrix();
            Gl.glLineWidth(4);
            Gl.glColor3d(0, 0, 255);
            Gl.glBegin(Gl.GL_LINES);

            Gl.glVertex3d(3.5, 1f, 0.0f);
            Gl.glVertex3d(4.5, positionLightningY + 1, 0.2);
            Gl.glVertex3d(4.5, positionLightningY + 1, 0.2);
            Gl.glVertex3d(5.3, positionLightningY + 0.45, 0.4);
            Gl.glVertex3d(5.3, positionLightningY + 0.45, 0.4);
            Gl.glVertex3d(5.9, positionLightningY + 0.75, 0.4);

            Gl.glEnd();

            Gl.glPopMatrix();
        }

        private void RenderTimer_Tick(object sender, EventArgs e)
        {
            Draw();

            if (!isAtPlace)
            {
                if (angleForearmLeft > 30 || angleForearmRight > 30)
                {
                    isMove = true;
                    stepAngleArm = -stepAngleArm;
                }

                if (angleForearmRight < 0 && !isLeftArm)
                {
                    stepAngleArm = -stepAngleArm;
                    angleForearmLeft = 0;
                    isLeftArm = true;
                    isMove = false;
                }

                if (angleForearmLeft < 0 && isLeftArm)
                {
                    stepAngleArm = -stepAngleArm;
                    angleForearmRight = 0;
                    isLeftArm = false;
                    isMove = false;
                }
                if (isLeftArm)
                    angleForearmLeft += stepAngleArm;
                else angleForearmRight += stepAngleArm;

                if (isMove)
                    move += 0.05f;

                if (move > 6.6)
                    isAtPlace = true;
            }
            else
            {
                if (angleForearmRight < 70)
                    angleForearmRight -= stepAngleArm;

                if (angleArm < 70)
                    angleArm -= stepAngleArm;

                isTimeToPress = angleForearmRight == 70;

                if (isTimeToPress && movePress < 1.6)
                    movePress += 0.03;
            }

            if (movePress > 1 && movePress < 1.6)
            {
                isLightning = true;

                if (movePress <= 1.2)
                    positionLightningY = 0.3;

                else if (movePress <= 1.4)
                    positionLightningY = 0.1;

                else positionLightningY = -0.4;


                if (angleHead < 90)
                {
                    angleHead += 1.4f;
                    moveHeadZ -= 0.004f;
                    moveHeadY += 0.0045f;
                    moveHeadX -= 0.00045f;
                }
                scaleTerminatorY -= 0.02;
                scaleTerminatorZ += 0.01;
                moveTerminatorY -= 0.01;
            }
            else isLightning = false;

            isEyeDown = movePress >= 1.6;

            label10.Text = "Angle=" + angleForearmRight;
            label11.Text = "Move = " + movePress;
        }

        private void AnT_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta < 0 && zoom > 0)
                zoom -= 0.02f;
            else zoom += 0.02f;
        }

        private void AnT_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                angleX = e.X;
                angleY = e.Y;
            }
        }

        private double prev_x = 0, prev_y = 0, angleX, angleY;

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
