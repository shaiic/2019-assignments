import sys
from PyQt5.QtWidgets import QApplication, QWidget,QPushButton,QLabel,QMessageBox
from PyQt5.QtGui import QIcon,QImage,QPainter
from PyQt5.Qt import QLineEdit
from PyQt5.QtCore import pyqtSlot
import cv2
from PIL import Image
import requests
from io import BytesIO
import numpy as np

from onnxruntime_predict import ONNXRuntimeObjectDetection

def get_bear_detection_model():
    MODEL_FILENAME = 'Bear.onnx'
    LABELS_FILENAME = 'labels.txt'
    with open(LABELS_FILENAME, 'r') as f:
        labels = [l.strip() for l in f.readlines()]

    return ONNXRuntimeObjectDetection(MODEL_FILENAME, labels)


class BearDetectionWidget(QWidget):

    def __init__(self):
        super().__init__()
        self.mImage = None
        self.mQImage = QImage()
        self.mDetectionModel = get_bear_detection_model() 
        self.initUI()


    def initUI(self):

        self.setGeometry(200, 100, 600, 600)
        self.setWindowTitle('Bear Recognizer')
        self.setWindowIcon(QIcon('icon.jpg'))        
        ## and hint
        hint = QLabel(self)
        hint.setText("输入要识别的图片地址")
        hint.move(10, 20)
  
        ##create text box
        self.textbox = QLineEdit(self)
        self.textbox.move(20, 50)
        self.textbox.resize(400, 30)

        ##add button
        self.button = QPushButton('添加图片', self)
        self.button.move(60, 90)   
        self.button.clicked.connect(self.on_load_image)

        ##add button
        self.button = QPushButton('识别', self)
        self.button.move(200, 90)   
        self.button.clicked.connect(self.on_detect_bear)

        ## and result label
        self.result_hint = QLabel(self)
        self.result_hint.setText("                            ")
        self.result_hint.move(350, 100)
     

        self.show()


    def get_cv2image(self,image):
        cv2_image = cv2.cvtColor(np.array(image), cv2.COLOR_RGB2BGR)
        cv2_image = cv2.resize(cv2_image, (416, 416), interpolation=cv2.INTER_CUBIC)
        return cv2_image

    def get_qimage(self,image):
        height, width, colors = image.shape
        bytesPerLine = 3 * width
        image = QImage(image.data, width, height, bytesPerLine, QImage.Format_RGB888)
        image = image.rgbSwapped()
        return image

    def get_image_from_url(self,url):
        response = requests.get(url)
        img = Image.open(BytesIO(response.content))
        return img

    @pyqtSlot()
    def on_load_image(self):
        textboxValue = self.textbox.text()
        try:
            self.mImage = self.get_image_from_url(textboxValue)
            cv2_image = self.get_cv2image(self.mImage)
            self.mQImage = self.get_qimage(cv2_image)
            self.update()
        except:
            QMessageBox.information(self,"错误","图片加载失败",QMessageBox.Yes)

    @pyqtSlot()
    def on_detect_bear(self):
        if self.mImage is None:
            QMessageBox.information(self,"错误","请先加载图片",QMessageBox.Yes)
            return
        predictions = self.mDetectionModel.predict_image(self.mImage)
        predictions.sort(key = lambda k:k['probability'],reverse=True)
        pred = predictions[0]
        cv2_image = self.get_cv2image(self.mImage)
        (x,y,w,h) = [int(i*416) for i in pred.get('boundingBox').values()]
        
        cv2.rectangle(cv2_image, (x, y), (x+w, y+h), (0, 0, 255), 1)
        hint_text = pred.get('tagName')+': '+str(pred.get('probability'))
        cv2.putText(cv2_image,hint_text,(x,y+min(w,10)),cv2.FONT_HERSHEY_SIMPLEX,0.5,(0,255,0),1)
        self.mQImage = self.get_qimage(cv2_image)

        #self.result_hint.setText(pred.get('tagName')+': '+str(pred.get('probability'))) 
        self.update()

    def paintEvent(self, QPaintEvent):
        painter = QPainter()
        painter.begin(self)
        painter.drawImage(10, 150, self.mQImage)
        painter.end()


if __name__ == '__main__':

    app = QApplication(sys.argv)
    ex = BearDetectionWidget()
    sys.exit(app.exec_())