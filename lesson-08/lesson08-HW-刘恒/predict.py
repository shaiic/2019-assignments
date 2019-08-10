
import sys
import tensorflow as tf
import numpy as np
import cv2
from PIL import Image, ImageFilter

def predictint(imvalue):
   
    # Define the model (same as when creating the model file)
    x = tf.placeholder(tf.float32, [None, 784])
    W = tf.Variable(tf.zeros([784, 10]))
    b = tf.Variable(tf.zeros([10]))
    
    def weight_variable(shape):
      initial = tf.truncated_normal(shape, stddev=0.1)
      return tf.Variable(initial)
    
    def bias_variable(shape):
      initial = tf.constant(0.1, shape=shape)
      return tf.Variable(initial)
       
    def conv2d(x, W):
      return tf.nn.conv2d(x, W, strides=[1, 1, 1, 1], padding='SAME')
    
    def max_pool_2x2(x):
      return tf.nn.max_pool(x, ksize=[1, 2, 2, 1], strides=[1, 2, 2, 1], padding='SAME')   
    
    W_conv1 = weight_variable([5, 5, 1, 32])
    b_conv1 = bias_variable([32])
    
    x_image = tf.reshape(x, [-1,28,28,1])
    h_conv1 = tf.nn.relu(conv2d(x_image, W_conv1) + b_conv1)
    h_pool1 = max_pool_2x2(h_conv1)
    
    W_conv2 = weight_variable([5, 5, 32, 64])
    b_conv2 = bias_variable([64])
    
    h_conv2 = tf.nn.relu(conv2d(h_pool1, W_conv2) + b_conv2)
    h_pool2 = max_pool_2x2(h_conv2)
    
    W_fc1 = weight_variable([7 * 7 * 64, 1024])
    b_fc1 = bias_variable([1024])
    
    h_pool2_flat = tf.reshape(h_pool2, [-1, 7*7*64])
    h_fc1 = tf.nn.relu(tf.matmul(h_pool2_flat, W_fc1) + b_fc1)
    
    keep_prob = tf.placeholder(tf.float32)
    h_fc1_drop = tf.nn.dropout(h_fc1, keep_prob)
    
    W_fc2 = weight_variable([1024, 10])
    b_fc2 = bias_variable([10])
    
    y_conv=tf.nn.softmax(tf.matmul(h_fc1_drop, W_fc2) + b_fc2)
    
    init_op = tf.global_variables_initializer()

   
    model_path='/Users/liuheng/Desktop/ai-course/lesson8/output'
    # model_path='/Users/liuheng/Desktop/ai-course/lesson8/MNIST/mnist_org/output'
    ckpt = tf.train.get_checkpoint_state(model_path + '/')
    saver = tf.train.import_meta_graph(ckpt.model_checkpoint_path + '.meta') 
    with tf.Session() as sess:
        sess.run(init_op)
        saver.restore(sess, ckpt.model_checkpoint_path)
        #print ("Model restored.")
       
        prediction=tf.argmax(y_conv,1)
        return prediction.eval(feed_dict={x: [imvalue],keep_prob: 1.0}, session=sess)



# def predict(imvalue):
#     # model_path='/Users/liuheng/Desktop/ai-course/lesson8/output'
#     model_path='/Users/liuheng/Desktop/ai-course/lesson8/MNIST/mnist_org/output'
#     ckpt = tf.train.get_checkpoint_state(model_path + '/')
#     saver = tf.train.import_meta_graph(ckpt.model_checkpoint_path + '.meta')    
    
#     with tf.Session() as sess:
       
       
#         saver.restore(sess, ckpt.model_checkpoint_path)
#         graph = tf.get_default_graph()   

#         image_input = graph.get_operation_by_name('image_input').outputs[0]

#         tensor_name_list = [tensor.name for tensor in tf.get_default_graph().as_graph_def().node]

#         prediction = sess.run(image_input, {'image_input:0': imvalue})
#         prediction=tf.argmax(prediction,1)

#         return  prediction.eval()


# def reserveImg(img):
#     for i in range(img.shape[0]):
#         for j in range(img.shape[1]):
#             img[i, j] = 255 - img[i, j]
#     return img


# def pre_pc(src):
#     img=Image.open(src)
#     reIm=img.resize((28,28),Image.ANTIALIAS)
#     im_arr=np.array(reIm.convert('L'))
#     threshold=50
#     for i in range(28):
#       for j in range(28):
#         im_arr[i][j]=255-im_arr[i][j]
#         if(im_arr[i][j]<threshold):
#            im_arr[i][j]=0
#         else: im_arr[i][j]=255
#     nm_arr=im_arr.reshape([1,784])
#     nm_arr=nm_arr.astype(np.float32)
#     img_ready=np.multiply(nm_arr,1.0/255.0)

#     return img_ready

# def create_img(src):
#     img_rows, img_cols = 28, 28
#     img = cv2.imread(src,0)


#     retval, im_at_fixed = cv2.threshold(img, 130, 255, cv2.THRESH_TOZERO)

#     # cv2.imshow('1',im_at_fixed)
#     img = reserveImg(im_at_fixed)
#     # cv2.imshow('2',img)
#     cv2.waitKey()

#     img = cv2.resize(img, (img_rows, img_cols), interpolation=cv2.INTER_CUBIC)
#     img = np.array([img])
#     img = img.astype('float32')

#     img = img.reshape(img.shape[0], img_rows, img_cols, 1)

#     img = img/255
#     return img

def imageprepare(argv):
    
    im = Image.open(argv).convert('L')
    width = float(im.size[0])
    height = float(im.size[1])
    newImage = Image.new('L', (28, 28), (255)) #creates white canvas of 28x28 pixels
    
    if width > height: #check which dimension is bigger
        #Width is bigger. Width becomes 20 pixels.
       
        nheight = int(round((20.0/width*height),0)) #resize height according to ratio width
      
        # resize and sharpen
        img = im.resize((20,nheight), Image.ANTIALIAS).filter(ImageFilter.SHARPEN)
        wtop = int(round(((28 - nheight)/2),0)) #caculate horizontal pozition
        newImage.paste(img, (4, wtop)) #paste resized image on white canvas
    else:
        #Height is bigger. Heigth becomes 20 pixels. 
        nwidth = int(round((20.0/height*width),0)) #resize width according to ratio height
        if (nwidth == 0): #rare case but minimum is 1 pixel
            nwidth = 1
         # resize and sharpen
        img = im.resize((nwidth,20), Image.ANTIALIAS).filter(ImageFilter.SHARPEN)
        wleft = int(round(((28 - nwidth)/2),0)) #caculate vertical pozition
        newImage.paste(img, (wleft, 4)) #paste resized image on white canvas
    
    #newImage.save("sample.png")

    tv = list(newImage.getdata()) #get pixel values
    
    #normalize pixels to 0 and 1. 0 is pure white, 1 is pure black.
    tva = [ (255-x)*1.0/255.0 for x in tv] 
    return tva
    #print(tva)
  


def main(argv):
    """
    Main function.
    """
    # img = pre_pc(argv)
    # print(img)

    # img=create_img(argv)
    img=imageprepare(argv)

    p = predictint(img)
    print (p) 
    
if __name__ == "__main__":
    #main(sys.argv[1])
     main("/Users/liuheng/Desktop/ai-course/lesson8/images/9.png")  
