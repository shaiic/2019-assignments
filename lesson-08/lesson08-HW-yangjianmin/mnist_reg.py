from PIL import Image
import tensorflow as tf
import os

META_FILE = 'form/model.ckpt.meta'
CKPT_FILE = 'form/model.ckpt'
IMAGE_PATH = 'images/'

def getAllImgFile(IMAGE_PATH):
    fileList = os.listdir(IMAGE_PATH)
    for fileName in fileList:
        absFile = os.path.join(IMAGE_PATH, fileName)
        print(absFile)


def img_convert(image_file):
    img = Image.open(image_file)
    img = img.resize((28, 28), Image.ANTIALIAS).convert('L')
    img_data = [(255 - x) * 1.0 / 255.0 for x in list(img.getdata())]
    return img_data

def img_predict():
    init = tf.compat.v1.global_variables_initializer()
    saver = tf.compat.v1.train.Saver
    with tf.compat.v1.Session() as sess:
        sess.run(init)
        saver = tf.compat.v1.train.import_meta_graph(META_FILE)
        saver.restore(sess, CKPT_FILE)

        graph = tf.compat.v1.get_default_graph()  # 加载计算图
        x = graph.get_tensor_by_name("x:0")  # 从模型中读取占位符张量
        keep_prob = graph.get_tensor_by_name("keep_prob:0")
        y_conv = graph.get_tensor_by_name("y_conv:0")  # 从模型中读取占位符变量

        prediction = tf.argmax(y_conv, 1)
        fileList = os.listdir(IMAGE_PATH)
        for fileName in fileList:
            absFile = os.path.join(IMAGE_PATH, fileName)
            image = img_convert(absFile)
            predint = prediction.eval(feed_dict={x: [image], keep_prob: 1.0}, session=sess)
            print('输入图片：',absFile,'模型预测值：',predint[0])  # 预测结果

if __name__ == '__main__':
    img_predict()

