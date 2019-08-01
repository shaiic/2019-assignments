from __future__ import absolute_import, unicode_literals
from tensorflow.examples.tutorials.mnist import input_data
import tensorflow as tf
import numpy as np
import matplotlib.pyplot as plt
from PIL import Image
plt.switch_backend('agg')
#主函数
if __name__ == '__main__':
    #载入mnist数据集
    #mnist = input_data.read_data_sets("./input/", one_hot=True)
    with tf.Session() as sess:
        #加载模型
        tf.saved_model.loader.load(sess, [tf.saved_model.tag_constants.SERVING], './export/')
        #初始化
        sess.run(tf.global_variables_initializer())
        #获取输入tensor
        input = sess.graph.get_tensor_by_name("image_input:0")
        #获取输出tensor
        output = sess.graph.get_tensor_by_name("predict_op:0")
        #mnist图像取一个测试
        #image=mnist.test.images[0].reshape(1,28,28,1);
        #转到-0.5到0.5之间
        #image=image-0.5
        #开始推理
        #y_conv_2 = sess.run(output, feed_dict={input:image})
        #标签值
        #y_2 = mnist.test.labels[0]
        #输出结果
        #print(y_conv_2[0],np.argmax(y_2))
        #输出推理结果（正确或者错误）
        #correct_prediction_2 = tf.equal(tf.argmax(y_conv_2, 1), tf.argmax(y_2, 1))
        #读取一个图片
        pil_im = Image.open('./9.png');
        #转成灰度值
        pil_im=pil_im.convert('L')
        #resize
        pil_im=pil_im.resize((28,28))
        #转成矩阵
        pil_im = np.array(pil_im,'f')
        #反相处理
        pil_im = 255 - pil_im
        #规一化到0到1之间
        pil_im = pil_im // 255;
        #转到到-0.5到0.5之间
        pil_im=0.5-pil_im;
        #转成tensor
        pil_im=pil_im.reshape(1,28,28,1);
        #推理
        y_conv_2 = sess.run(output, feed_dict={input:pil_im})
        #输出结果
        print(y_conv_2[0])

