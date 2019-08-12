import sys
from predicator import *
from PyQt5.QtWidgets import (QApplication, QWidget, QLabel, QPushButton)
from PyQt5.QtGui import (QPainter, QPen, QFont, QPalette)
from PyQt5.QtCore import Qt
from PIL import ImageGrab, Image

class Drawing(QWidget):
 
 def __init__(self):
  super(Drawing, self).__init__()
 
  self.resize(284, 330)  # resize设置宽高
  self.move(100, 100)    # move设置位置
  self.setFixedSize(284, 330)
  self.setWindowTitle("nmist模型验证")
  # self.setWindowFlags(Qt.WindowMinMaxButtonsHint|QT.cl) 

  self.setMouseTracking(False)
  self.pos_xy = []

  # 添加一系列控件
  self.label_draw = QLabel('', self)
  self.label_draw.setGeometry(2, 2, 280, 280)  
  self.label_draw.setStyleSheet("QLabel{border:1px solid black;}")
  self.label_draw.setAlignment(Qt.AlignCenter)

  self.label_result_name = QLabel('识别结果：', self)
  self.label_result_name.setGeometry(2, 290, 60, 35)
  self.label_result_name.setAlignment(Qt.AlignCenter)

  self.label_result = QLabel(' ', self)
  self.label_result.setGeometry(64, 290, 35, 35)
  self.label_result.setFont(QFont("Roman times", 8, QFont.Bold))
  self.label_result.setStyleSheet("QLabel{border:1px solid black;}")
  self.label_result.setAlignment(Qt.AlignCenter)


  self.btn_clear = QPushButton("清空", self)
  self.btn_clear.setGeometry(170, 290, 50, 35)
  self.btn_clear.clicked.connect(self.btn_clear_on_clicked)

  self.btn_close = QPushButton("识别", self)
  self.btn_close.setGeometry(230, 290, 50, 35)
  self.btn_close.clicked.connect(self.btn_recognize_on_clicked)

 def btn_recognize_on_clicked(self):
  x = 104
  y = 134
  lengh = 278
  x1 = x + lengh
  y1 = y + lengh
  im = ImageGrab.grab((x, y, x1, y1)).convert('L')
  px = im.load()
  print(px)
  for i in range(im.width):
    for j in range(im.height):
      if px[j, i] <= 250:
        im.putpixel((j, i), 0)
  im.save('./temp.jpg')
  result = predicateFile('./temp.jpg')
  self.label_result.setText(str(result))

 def btn_clear_on_clicked(self):
   self.pos_xy = []
   self.label_result.setText('')
   self.update()

 def paintEvent(self, event):
  painter = QPainter()
  painter.begin(self)
  pen = QPen(Qt.white, 6, Qt.SolidLine)
  painter.setPen(pen)

  if len(self.pos_xy) > 1:
   point_start = self.pos_xy[0]
   for pos_tmp in self.pos_xy:
    point_end = pos_tmp
 
    if point_end == (-1, -1):
     point_start = (-1, -1)
     continue
    if point_start == (-1, -1):
     point_start = point_end
     continue
 
    painter.drawLine(point_start[0], point_start[1], point_end[0], point_end[1])
    point_start = point_end
  painter.end()
 
 def mouseMoveEvent(self, event):
  pos_tmp = (event.pos().x(), event.pos().y())
  self.pos_xy.append(pos_tmp)
 
  self.update()
 
 def mouseReleaseEvent(self, event):
  pos_test = (-1, -1)
  self.pos_xy.append(pos_test)
 
  self.update()
 
if __name__ == "__main__":
 app = QApplication(sys.argv)
 pyqt_learn = Drawing()
 pyqt_learn.show()
 app.exec_()
