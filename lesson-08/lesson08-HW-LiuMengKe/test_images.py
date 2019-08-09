import sys
import argparse

from keras.models import load_model
import cv2
import numpy as np

def main(args=None):

    image_row_num, image_column_num = 28, 28
    if args is None:
        args = sys.argv[1:]

    # Parse input parameter
    parser = argparse.ArgumentParser()
    parser.add_argument("--image",
            help="image file path",
            default="")
    parser.add_argument("--model",
            help="keras model",
            default="")
    args = parser.parse_args(args)

    # Pre-process test image
    img = cv2.imread(args.image,0)
    img = cv2.resize(img,(image_row_num, image_column_num))
    img = img.reshape(1,img.shape[0], img.shape[1],1)
    img = img.astype('float32')
    img = img / 255

    # load trained model.
    model = load_model(args.model)

    # predict.
    result = np.argmax(model.predict(img)[0])

    # output result
    print('Image name: %s, Predict result: %s' % (args.image, result))

if __name__ == "__main__":
    main()
