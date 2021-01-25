#include <stdio.h>
#include <stdlib.h>
#include <windows.h>
#include <GL/glut.h>
#include <glm/glm.hpp>
#include <vector>
#include <iostream>
#include <fstream>
#include <string>
#include <vector>
#include <boost/algorithm/string.hpp> 
#include <limits>
using namespace std;
using namespace boost;

void display();
void reshape(int width, int height);
void renderScene();
double generirajBroj(int indeks, int dodatno);
glm::dvec4 tVec(float t);
glm::dvec4 calculatePoint();
glm::dvec3 tVec2(float t);
glm::dvec4 calculateDerivation();
glm::dvec4 calculateDerivation2();
glm::dmat3x3 calculateDCM();
glm::dvec2 tVec3(float t);
vector<glm::dvec3> setupPoints();

bool isNegative = false;
float tangentScale = 1;
float speed = 0.1f;
bool boja = true;
bool linija = true;
int width = 400, height = 400;
vector<glm::ivec3> poligoni;
vector<glm::dvec3> tocke;
vector<glm::dvec4> bSplinePoints;
int tockeCnt = 0;
int poligoniCnt = 0;
double xmin, xmax, ymin, ymax, zmin, zmax;
glm::dvec3 srediste;
glm::dmat4x4 bCubeMat = { -1,3,-3,1,3,-6,0,4,-3,3,3,1,1,0,0,0 };
glm::dmat4x3 derMat = { -1,2,-1,3,-4,0,-3,2,1,1,0,0 };
glm::dmat4x2 derMat2 = {-1, 1, 3, -2, -3, 1, 1, 0 };
vector<glm::dmat4x4> segmentMat;
vector<glm::dvec4> spline;
float currentT;
int currentSegment;

void specialKey(int key, int x, int y) {

	if (key == GLUT_KEY_UP) {
		currentT += speed;
		isNegative = false;
		if (currentT >= 1)
		{
			if (currentSegment < segmentMat.size()-1)
			{
				currentT = 0;
				currentSegment++;
			}
			else
				currentT = 1;
		}
	}
	if (key == GLUT_KEY_DOWN) {
		currentT -= speed;
		isNegative = true;
		if (currentT <= 0)
		{
			if (currentSegment > 0)
			{
				currentT = 1;
				currentSegment--;
			}
			else
				currentT = 0;
		}
	}
	glutPostRedisplay();
}

void keyPressed(unsigned char key, int x, int y) {
	if (key == 'b')
		boja = !boja;
	if (key == 'l')
		linija = !linija;
	glutPostRedisplay();
}
int main(int argc, char* argv[]) {

	xmin = std::numeric_limits<double>::max();
	ymin = std::numeric_limits<double>::max();
	zmin = std::numeric_limits<double>::max();
	xmax = -std::numeric_limits<double>::max();
	ymax = -std::numeric_limits<double>::max();
	zmax = -std::numeric_limits<double>::max();
	char ime1[50];
	printf("Sa tipkom L ugasite prikaz linija,a sa tipkom B ugasite prikaz boje.\nUpisite ime datoteke koju zelite prikazati : ");
	scanf_s("%s", ime1, 50);
	string ime = ime1;
	ifstream datoteka1(ime);
	string linija;
	if (datoteka1.is_open()) {
		while (getline(datoteka1, linija)) {
			if (linija._Equal(""))
				continue;
			if (linija.at(0) == '#')
				continue;
			std::vector<string> podijeljeniRedak;

			boost::split(podijeljeniRedak, linija, boost::is_any_of(" "));
			if (podijeljeniRedak[0]._Equal("v")) {
				++tockeCnt;
			}
			else {
				++poligoniCnt;
			}
		}
	}
	datoteka1.close();
	tocke.reserve(tockeCnt);
	poligoni.reserve(poligoniCnt);
	ifstream datoteka(ime);
	if (datoteka.is_open()) {
		while (getline(datoteka, linija)) {

			if (linija._Equal(""))
				continue;
			if (linija.at(0) == '#')
				continue;
			vector<string> podijeljeniRedak;

			boost::split(podijeljeniRedak, linija, boost::is_any_of(" "));
			if (podijeljeniRedak[0]._Equal("v")) {
				glm::dvec3 tocka;
				tocka.x = std::stod(podijeljeniRedak[1]);
				tocka.y = std::stod(podijeljeniRedak[2]);
				tocka.z = std::stod(podijeljeniRedak[3]);

				tocke.push_back(tocka);
				if (tocka.x > xmax)
					xmax = tocka.x;
				if (tocka.y > ymax)
					ymax = tocka.y;
				if (tocka.z > zmax)
					zmax = tocka.z;
				if (tocka.x < xmin)
					xmin = tocka.x;
				if (tocka.y < ymin)
					ymin = tocka.y;
				if (tocka.z < zmin)
					zmin = tocka.z;
			}
			else if (podijeljeniRedak[0]._Equal("f")) {
				glm::ivec3 poligon;
				glm::dvec4 poligonJedn;
				poligon.x = std::stoi(podijeljeniRedak[1]);
				poligon.y = std::stoi(podijeljeniRedak[2]);
				poligon.z = std::stoi(podijeljeniRedak[3]);
				poligoni.push_back(poligon);
			}

		}
		datoteka.close();
	}
	else {
		printf("Nije moguæe otvoriti datoteku.");
	}

	ifstream file("bspirala.txt");
	string line;

	if (file.is_open()) {
		while (getline(file, line)) {
			if (line._Equal(""))
				continue;
			std::vector<string> podijeljeniRedak;

			boost::split(podijeljeniRedak, line, boost::is_any_of(" "));
			glm::dvec4 point;

			point.x = stod(podijeljeniRedak[0]);
			point.y = stod(podijeljeniRedak[1]);
			point.z = stod(podijeljeniRedak[2]);
			point.w = 1;
			bSplinePoints.push_back(point);
		}
	}
	else 
	{
		printf("Ne mogu otvoriti file.");
	}
	file.close();
	int numOfSegments = bSplinePoints.size() - 3;
	segmentMat.reserve(numOfSegments);

	for (int i = 0; i < numOfSegments; ++i) {
		glm::mat4x4 segm = { bSplinePoints[i],bSplinePoints[i+1], bSplinePoints[i+2], bSplinePoints[i+3] };
		segmentMat.push_back(glm::transpose(segm));
	}

	for (int i = 0; i < numOfSegments; ++i) 
	{
		for (float t = 0; t <= 1; t += 0.01f) 
		{
			currentSegment = i;
			currentT = t;
			spline.push_back(calculatePoint());
		}
	}

	currentSegment = 0;
	currentT = 0;

	srediste.x = xmin + (xmax - xmin) / 2;
	srediste.y = ymin + (ymax - ymin) / 2;
	srediste.z = zmin + (zmax - zmin) / 2;

	for (int i = 0; i < tocke.size(); ++i)
		tocke[i] = glm::dvec3(tocke[i].x - srediste.x, tocke[i].y - srediste.y, tocke[i].z - srediste.z);

	printf("%f %f %f", srediste.x, srediste.y, srediste.z);
	glutInit(&argc, argv);
	glutInitDisplayMode(GLUT_DOUBLE | GLUT_DEPTH);
	glutInitWindowSize(width, height);
	glutInitWindowPosition(0, 0);
	glutCreateWindow("1. zadatak RG");
	glutSpecialFunc(specialKey);
	glutDisplayFunc(display);
	glutReshapeFunc(reshape);
	glutKeyboardFunc(keyPressed);
	glutMainLoop();
	return 0;
}

void reshape(int w, int h) {
	width = w;
	height = h;
	glEnable(GL_DEPTH_TEST);
	glViewport(0, 0, (GLsizei)width, (GLsizei)height);
	glMatrixMode(GL_PROJECTION);
	glLoadIdentity();
	glOrtho(-2, 2, -2, 2, -2, 2);
	glMatrixMode(GL_MODELVIEW);
}

void display() {
	glClearColor(0, 0, 0, 1);
	glClear(GL_DEPTH_BUFFER_BIT | GL_COLOR_BUFFER_BIT);
	glLoadIdentity();
	renderScene();
	glutSwapBuffers();
}

void renderScene() {
	glPointSize(10.0f);
	glColor3f(1, 1, 1);
	glBegin(GL_LINES);
	vector<glm::dvec3> points = setupPoints();

	for (glm::dvec4& spPoint : spline) 
	{
		glVertex3d(spPoint.x, spPoint.y, spPoint.z);
	}
	glEnd();

	glBegin(GL_LINES);
	{
		glm::dvec3 shiftVec = (glm::dvec3)calculatePoint();
		glVertex3d(shiftVec.x, shiftVec.y, shiftVec.z);
		glm::dvec3 shiftedShiftVec = shiftVec + (glm::dvec3)calculateDerivation();
		glVertex3d(shiftedShiftVec.x, shiftedShiftVec.y, shiftedShiftVec.z);
	}
	glEnd();
	
	if (boja) {
		glBegin(GL_TRIANGLES);
		for (glm::ivec3& trokut : poligoni) {
				glColor3f(generirajBroj(2 - 2 * points[trokut.x-1].x, 0), generirajBroj(1 - points[trokut.x - 1].y, 1), generirajBroj(3 - 3 * points[trokut.x - 1].z, 2));
				glVertex3d(points[trokut.x - 1].x, points[trokut.x - 1].y, points[trokut.x - 1].z );
				glColor3f(generirajBroj(2 - 2 * points[trokut.y - 1].x, 0), generirajBroj(1 - points[trokut.y - 1].y, 1), generirajBroj(3 - 3 * points[trokut.y - 1].z, 2));
				glVertex3d(points[trokut.y - 1].x, points[trokut.y - 1].y, points[trokut.y - 1].z);
				glColor3f(generirajBroj(2 - 2 * points[trokut.z - 1].x, 0), generirajBroj(1 - points[trokut.z - 1].y, 1), generirajBroj(3 - 3 * points[trokut.z - 1].z, 2));
				glVertex3d(points[trokut.z - 1].x, points[trokut.z - 1].y, points[trokut.z - 1].z);
		}
		glEnd();
	}

	if (linija) {
		glColor3f(1, 1, 1);
		glBegin(GL_LINE_LOOP);
		for (glm::ivec3& trokut : poligoni) {
			glVertex3d(points[trokut.x - 1].x, points[trokut.x - 1].y, points[trokut.x - 1].z);
			glVertex3d(points[trokut.y - 1].x, points[trokut.y - 1].y, points[trokut.y - 1].z);
			glVertex3d(points[trokut.z - 1].x, points[trokut.z - 1].y, points[trokut.z - 1].z);
		}
		glEnd();
	}
	glFlush();
}

glm::dvec4 tVec(float t) 
{
	return glm::dvec4(pow(t, 3), pow(t, 2), pow(t, 1), 1);
}

glm::dvec3 tVec2(float t)
{
	return glm::dvec3(pow(t, 2), pow(t, 1), 1);
}

glm::dvec2 tVec3(float t)
{
	return glm::dvec2(pow(t, 2), 1);
}


glm::dvec4 calculatePoint() 
{
	glm::dmat4x4 scalarMat = { 1. / 6,1. / 6, 1. / 6, 1. / 6, 1. / 6, 1. / 6, 1. / 6, 1. / 6, 1. / 6, 1. / 6, 1. / 6, 1. / 6, 1. / 6, 1. / 6, 1. / 6, 1. / 6 };
	return tVec(currentT) * glm::matrixCompMult(scalarMat,bCubeMat) * segmentMat[currentSegment];
}

glm::dvec4 calculateDerivation()
{
	float constant = (isNegative ? -1 : 1)*0.5f;
	glm::dmat4x3 scalarMat = { constant,constant, constant, constant, constant, constant, constant, constant, constant, constant, constant, constant};
	return tVec2(currentT) * glm::matrixCompMult(scalarMat, derMat) * segmentMat[currentSegment];
}

glm::dvec4 calculateDerivation2()
{
	float constant = (isNegative ? -1 : 1);
	glm::dmat4x2 scalarMat = { constant,constant, constant, constant, constant, constant, constant, constant};
	return tVec3(currentT) * glm::matrixCompMult(scalarMat, derMat2) * segmentMat[currentSegment];
}

double generirajBroj(int indeks, int dodatno)
{
	return pow(sin(indeks), 2 / 3)*sin(pow(indeks, dodatno));
}

glm::dmat3x3 calculateDCM() 
{
	glm::dvec3 derivation = (glm::dvec3)calculateDerivation();
	glm::dvec3 derivation2 = (glm::dvec3)calculateDerivation2();
	glm::dvec3 binormal = glm::cross(derivation,derivation2);
	glm::dmat3x3 dcm = { derivation.x, derivation.y, derivation.z, binormal.x, binormal.y, binormal.z, derivation2.x, derivation2.y, derivation2.z };
	return dcm;
}

vector<glm::dvec3> setupPoints() 
{
	vector<glm::dvec3> points;
	glm::dvec3 shiftVec = (glm::dvec3)calculatePoint();
	for (glm::dvec3& point : tocke)
	{
		glm::dvec3 point1 = point * calculateDCM() + shiftVec;
		points.push_back(point1);
	}
	
	return points;
}
