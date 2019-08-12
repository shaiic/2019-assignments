from __future__ import print_function

import keras

from keras import backend as K
from keras.datasets import mnist
from keras.models import Sequential
from keras.layers import Dense, Dropout, Flatten
from keras.layers import Conv2D, MaxPooling2D

# Set the row and column for image.
image_row_num, image_column_num = 28, 28

# Set basic train parameters.
epochs = 10
num_classes = 10
batch_size = 200

#import pdb;pdb.set_trace()
# Get train and test data.
(XTrain, YTrain), (XTest, YTest) = mnist.load_data()

channel_type = K.image_data_format()

if channel_type == 'channels_first':
    XTrain = XTrain.reshape(XTrain.shape[0], 1, image_row_num, image_column_num)
    XTest = XTest.reshape(XTest.shape[0], 1, image_row_num, image_column_num)
    input_shape = (1, image_row_num, image_column_num)
elif channel_type == "channels_last":
    XTrain = XTrain.reshape(XTrain.shape[0], image_row_num, image_column_num, 1)
    XTest = XTest.reshape(XTest.shape[0], image_row_num, image_column_num, 1)
    input_shape = (image_row_num, image_column_num, 1)
else:
    exit(1)

XTrain = XTrain.astype('float32')
XTest = XTest.astype('float32')
XTrain /= 255
XTest /= 255
print('XTrain shape:', XTrain.shape)
print(XTrain.shape[0], 'train samples')
print(XTest.shape[0], 'test samples')

# Converts vector to class matrix.
YTrain = keras.utils.to_categorical(YTrain, num_classes)
YTest = keras.utils.to_categorical(YTest, num_classes)

model = Sequential()
model.add(Conv2D(32, kernel_size=(3, 3),
                 activation='relu',
                 input_shape=input_shape))
model.add(Conv2D(64, (3, 3), activation='relu'))
model.add(MaxPooling2D(pool_size=(2, 2)))
model.add(Dropout(0.25))
model.add(Flatten(channel_type))
model.add(Dense(128, activation='relu'))
model.add(Dropout(0.5))
model.add(Dense(num_classes, activation='softmax'))

model.compile(loss=keras.losses.categorical_crossentropy,
              optimizer=keras.optimizers.Adadelta(),
              metrics=['accuracy'])

model.fit(
          x = XTrain,
          y = YTrain,
          batch_size=batch_size,
          epochs=epochs,
          verbose=1,
          validation_data=(XTest, YTest))
score = model.evaluate(XTest, YTest, verbose=0)
# Save trained model
model.save("trained_model01")

print('The result based on trained model, loss: %s, accuracy: %s' % (score[0], score[1]))
