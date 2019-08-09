from __future__ import print_function

from builtins import range
from builtins import object
import numpy as np
import matplotlib.pyplot as plt
from past.builtins import xrange
from random import randint
class TwoLayerNet(object):
    """
    A two-layer fully-connected neural network. The net has an input dimension of
    N, a hidden layer dimension of H, and performs classification over C classes.
    We train the network with a softmax loss function and L2 regularization on the
    weight matrices. The network uses a ReLU nonlinearity after the first fully
    connected layer.

    In other words, the network has the following architecture:

    input - fully connected layer - ReLU - fully connected layer - softmax

    The outputs of the second fully-connected layer are the scores for each class.
    """
    #D 代表几个输入，此时是4
    #H 代表几个神经元，此时是10
    #N 代表几个样本数据 此时是5，5个样本
    #C 代表第二层几个神经元，也就是多分类的总共几个分类,即分类数，此时为3
    def __init__(self, input_size, hidden_size, output_size, std=1e-4):
        """
        Initialize the model. Weights are initialized to small random values and
        biases are initialized to zero. Weights and biases are stored in the
        variable self.params, which is a dictionary with the following keys:

        W1: First layer weights; has shape (D, H)(4,10)
        b1: First layer biases; has shape (H,)(10,0)
        W2: Second layer weights; has shape (H, C)(10,3)
        b2: Second layer biases; has shape (C,)(3,)

        Inputs:
        - input_size: The dimension D of the input data.4
        - hidden_size: The number of neurons H in the hidden layer.10
        - output_size: The number of classes C.3
        """
        #下面是二层神经网络的权重和偏移值的初始化
        self.params = {}
        self.params['W1'] = std * np.random.randn(input_size, hidden_size)
        self.params['b1'] = np.zeros(hidden_size)
        self.params['W2'] = std * np.random.randn(hidden_size, output_size)
        self.params['b2'] = np.zeros(output_size)
    #D 代表几个输入，此时是4
    #H 代表几个神经元，此时是10
    #N 代表几个样本数据 此时是5，5个样本
    #C 代表第二层几个神经元，也就是多分类的总共几个分类,即分类数，此时为3
    def loss(self, X, y=None, reg=0.0):
        """
        Compute the loss and gradients for a two layer fully connected neural
        network.

        Inputs:
        - X: Input data of shape (N, D). Each X[i] is a training sample.(5,4)
        - y: Vector of training labels. y[i] is the label for X[i], and each y[i] is
          an integer in the range 0 <= y[i] < C(3). This parameter is optional; if it
          is not passed then we only return scores, and if it is passed then we
          instead return the loss and gradients.
        - reg: Regularization strength.

        Returns:
        If y is None, return a matrix scores of shape (N, C)(5,3) where scores[i, c] is
        the score for class c on input X[i].

        If y is not None, instead return a tuple of:
        - loss: Loss (data loss and regularization loss) for this batch of training
          samples.
        - grads: Dictionary mapping parameter names to gradients of those parameters
          with respect to the loss function; has the same keys as self.params.
        """
        # Unpack variables from the params dictionary
        W1, b1 = self.params['W1'], self.params['b1']
        W2, b2 = self.params['W2'], self.params['b2']
        N, D = X.shape#(5,4)

        # Compute the forward pass
        scores = None
        #############################################################################
        # TODO: Perform the forward pass, computing the class scores for the input. #
        # Store the result in the scores variable, which should be an array of      #
        # shape (N, C).(5,3)                                                        #
        #############################################################################
        # *****START OF YOUR CODE (DO NOT DELETE/MODIFY THIS LINE)*****
        #第一层神经元的正向计算
        Z1=np.dot(X,W1)+b1 #X是5行4列，W1是4行10列，点乘后Z1是5行10列
        #经过激活函数ReLU
        A1=np.maximum(Z1,0) #A1的形状是5行10列
        #第二层神经元的正向计算
        Z=np.dot(A1,W2)+b2 #A是5行10列，W2是10行3列，点乘后A是5行3列
        #算出结果分数
        scores=Z
        # *****END OF YOUR CODE (DO NOT DELETE/MODIFY THIS LINE)*****

        # If the targets are not given then jump out, we're done
        if y is None:
            return scores

        # Compute the loss
        loss = None
        #############################################################################
        # TODO: Finish the forward pass, and compute the loss. This should include  #
        # both the data loss and L2 regularization for W1 and W2. Store the result  #
        # in the variable loss, which should be a scalar. Use the Softmax           #
        # classifier loss.                                                          #
        #############################################################################
        # *****START OF YOUR CODE (DO NOT DELETE/MODIFY THIS LINE)*****
        #通过softmax多分类函数
        shift_Z=Z-np.max(Z)
        exp_Z=np.exp(shift_Z)
        A=exp_Z/np.sum(exp_Z, axis=1, keepdims=True)#A是5行3列
        #交叉嫡损失函数，如果以索引值的方式给出，直接算loss
        correct_logprobs = -np.log(A[range(N), y]) #此时A[range(N), y]是一个一维数组
        #y是标签值，索引分别是0 1 2 2 1
        loss = np.sum(correct_logprobs) / N#平均一下,求得此批次（因为一次计算5个样本）的loss
        #权重正则化，目前还不明白是怎么回事，就当没有看见这个东西,但不能注掉
        loss += 1*reg*(np.sum(W1*W1) + np.sum(W2*W2))
        #http://blog.a-stack.com/2018/05/03/cs231n-lecture-4/#%E5%AE%9E%E7%8E%B0-lt-%E4%BB%A3%E7%A0%81-gt
        #下面的才是正儿八经的交叉嫡损失函数的计算过程，但由于不是one-hot形式的标签值，没有办法使用
        #P1=np.log(A)
        #P2=np.multiply(y,P1) #此时P2是5行1列
        #loss=np.sum(-P2)
        #loss=loss/N

        # *****END OF YOUR CODE (DO NOT DELETE/MODIFY THIS LINE)*****

        # Backward pass: compute gradients
        grads = {}
        #############################################################################
        # TODO: Compute the backward pass, computing the derivatives of the weights #
        # and biases. Store the results in the grads dictionary. For example,       #
        # grads['W1'] should store the gradient on W1, and be a matrix of same size #
        #############################################################################
        # *****START OF YOUR CODE (DO NOT DELETE/MODIFY THIS LINE)*****
        #反向传播到Z1

        dZ = A
        print("dz0",dZ)
        dZ[range(N), y]=dZ[range(N), y]-1#交叉熵损失函数对softmax求导后的公式-(1/a)*[a*(1-a)]=a-1,即dZ=A-1,但只对标签索引进行反向传播计算
        print("dz1",dZ)
        dZ /= N#dZ 5行3列
        print("dz2",dZ)
        dW2 = np.dot(Z1.T, dZ) + 2*reg * W2
        db2 = np.sum(dZ, axis=0)
        #反向传播到激活函数relu
        dh1 = np.dot(dZ, W2.T)#dh1 5行10列
        dh1[dh1 <= 0] = 0
        #反向传播到第一层
        dW1 = np.dot(X.T, dh1) + 2*reg * W1#dw14行10列
        db1 = np.sum(dh1, axis=0)
        # *****END OF YOUR CODE (DO NOT DELETE/MODIFY THIS LINE)*****
        grads["W2"]=dW2
        grads["b2"]=db2
        grads["W1"]=dW1
        grads["b1"]=db1
        return loss, grads
    #D 代表几个输入，此时是4
    #H 代表几个神经元，此时是10
    #N 代表几个样本数据 此时是5，5个样本
    #C 代表第二层几个神经元，也就是多分类的总共几个分类,即分类数，此时为3
    def train(self, X, y, X_val, y_val,
              learning_rate=1e-3, learning_rate_decay=0.95,
              reg=5e-6, num_iters=100,
              batch_size=200, verbose=False):
        """
        Train this neural network using stochastic gradient descent.#单样本随机梯度下降

        Inputs:
        - X: A numpy array of shape (N, D)(5,4) giving training data.
        - y: A numpy array f shape (N,)(5) giving training labels; y[i] = c means that
          X[i] has label c, where 0 <= c < C.
        - X_val: A numpy array of shape (N_val, D) giving validation data.
        - y_val: A numpy array of shape (N_val,) giving validation labels.
        - learning_rate: Scalar giving learning rate for optimization.
        - learning_rate_decay: Scalar giving factor used to decay the learning rate
          after each epoch.
        - reg: Scalar giving regularization strength.
        - num_iters: Number of steps to take when optimizing.
        - batch_size: Number of training examples to use per step.
        - verbose: boolean; if true print progress during optimization.
        """
        num_train = X.shape[0]#数据的总行数
        iterations_per_epoch = max(num_train / batch_size, 1)

        # Use SGD to optimize the parameters in self.model
        loss_history = []
        train_acc_history = []
        val_acc_history = []

        for it in range(num_iters):
            X_batch = None
            y_batch = None

            #########################################################################
            # TODO: Create a random minibatch of training data and labels, storing  #
            # them in X_batch and y_batch respectively.                             #
            #########################################################################
            # *****START OF YOUR CODE (DO NOT DELETE/MODIFY THIS LINE)*****
            size=randint(1,num_train)#产生一个随机数
            X_batch=X[0:size]
            y_batch=y[0:size]
            # *****END OF YOUR CODE (DO NOT DELETE/MODIFY THIS LINE)*****

            # Compute loss and gradients using the current minibatch
            loss, grads = self.loss(X_batch, y=y_batch, reg=reg)
            loss_history.append(loss)

            #########################################################################
            # TODO: Use the gradients in the grads dictionary to update the         #
            # parameters of the network (stored in the dictionary self.params)      #
            # using stochastic gradient descent. You'll need to use the gradients   #
            # stored in the grads dictionary defined above.                         #
            #########################################################################
            # *****START OF YOUR CODE (DO NOT DELETE/MODIFY THIS LINE)*****

            pass

            # *****END OF YOUR CODE (DO NOT DELETE/MODIFY THIS LINE)*****

            if verbose and it % 100 == 0:
                print('iteration %d / %d: loss %f' % (it, num_iters, loss))

            # Every epoch, check train and val accuracy and decay learning rate.
            if it % iterations_per_epoch == 0:
                # Check accuracy
                train_acc = (self.predict(X_batch) == y_batch).mean()
                val_acc = (self.predict(X_val) == y_val).mean()
                train_acc_history.append(train_acc)
                val_acc_history.append(val_acc)

                # Decay learning rate
                learning_rate *= learning_rate_decay

        return {
          'loss_history': loss_history,
          'train_acc_history': train_acc_history,
          'val_acc_history': val_acc_history,
        }

    def predict(self, X):
        """
        Use the trained weights of this two-layer network to predict labels for
        data points. For each data point we predict scores for each of the C
        classes, and assign each data point to the class with the highest score.

        Inputs:
        - X: A numpy array of shape (N, D) giving N D-dimensional data points to
          classify.

        Returns:
        - y_pred: A numpy array of shape (N,) giving predicted labels for each of
          the elements of X. For all i, y_pred[i] = c means that X[i] is predicted
          to have class c, where 0 <= c < C.
        """
        y_pred = None

        ###########################################################################
        # TODO: Implement this function; it should be VERY simple!                #
        ###########################################################################
        # *****START OF YOUR CODE (DO NOT DELETE/MODIFY THIS LINE)*****

        #第一层神经元的正向计算
        Z1=np.dot(X,W1)+b1 #X是5行4列，W1是4行10列，点乘后Z1是5行10列
        #经过激活函数ReLU
        A1=np.maximum(Z1,0) #A的形状是5行10列
        #第二层神经元的正向计算
        Z=np.dot(A1,W2)+b2 #A是5行10列，W2是10行3列，点乘后A是5行3列
        #算出结果分数
        y_pred=Z

        # *****END OF YOUR CODE (DO NOT DELETE/MODIFY THIS LINE)*****

        return y_pred
