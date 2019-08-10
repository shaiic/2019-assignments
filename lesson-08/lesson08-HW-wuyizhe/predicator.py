# Importing the Keras libraries and packages
import numpy as np
from PIL import Image
from keras.models import load_model
model = load_model('single_model.h5')

def readNum(pred):
  index = 0
  for item in pred.tolist()[0]:    
    if item > 0.5: 
      return index
    else:
      index += 1
  return -1

def predicateImage(img):
  img = img.resize((28, 28), Image.ANTIALIAS)
  im2arr = np.array(img)
  im2arr = im2arr.reshape(1, 28, 28, 1)
  # Predicting the Test set results
  y_pred = model.predict(im2arr)
  return readNum(y_pred)

def predicateFile(imgFile):  
  img = Image.open(imgFile).convert("L")
  return predicateImage(img)

