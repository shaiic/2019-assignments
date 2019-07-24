import numpy as np
import matplotlib.pyplot as plt
import math
from enum import Enum
from pathlib import Path
from matplotlib.colors import LogNorm

file_name = "./train.npz"

class NetType(Enum):
    Fitting = 1,
    BinaryClassifier = 2,
    MultipleClassifier = 3,


class HyperParameters(object):
    def __init__(self, input_size, output_size, eta=0.1, max_epoch=1000, batch_size=5, eps=0.1, net_type=NetType.Fitting):
        self.input_size = input_size
        self.output_size = output_size
        self.eta = eta
        self.max_epoch = max_epoch
        self.batch_size = batch_size
        self.eps = eps
        self.net_type = net_type

    def toString(self):
        title = str.format("in:{0},bz:{1},eta:{2}",
                           self.input_size, self.batch_size, self.eta)
        return title


class LossFunction(object):
    def __init__(self, net_type):
        self.net_type = net_type
    # end def

    # fcFunc: feed forward calculation
    def CheckLoss(self, A, Y):
        m = Y.shape[0]
        if self.net_type == NetType.Fitting:
            loss = self.MSE(A, Y, m)
        elif self.net_type == NetType.BinaryClassifier:
            loss = self.CE2(A, Y, m)
        elif self.net_type == NetType.MultipleClassifier:
            loss = self.CE3(A, Y, m)
        elif self.net_type == NetType.BinaryTanh:
            loss = self.CE2_tanh(A, Y, m)
        #end if
        return loss
    # end def

    def MSE(self, A, Y, count):
        p1 = A - Y
        LOSS = np.multiply(p1, p1)
        loss = LOSS.sum()/count/2
        return loss
    # end def

    # for binary classifier
    def CE2(self, A, Y, count):
        p1 = 1 - Y
        p2 = np.log(1 - A + 1e-5)
        p3 = np.log(A)

        p4 = np.multiply(p1, p2)
        p5 = np.multiply(Y, p3)

        LOSS = np.sum(-(p4 + p5))  # binary classification
        loss = LOSS / count
        return loss
    # end def

    # for multiple classifier
    def CE3(self, A, Y, count):
        p1 = np.log(A+1e-7)
        p2 = np.multiply(Y, p1)
        LOSS = np.sum(-p2)
        loss = LOSS / count
        return loss
    # end def

    # for binary tanh classifier
    def CE2_tanh(self, A, Y, count):
        #p = (1-Y) * np.log(1-A) + (1+Y) * np.log(1+A)
        p = (1-Y) * np.log((1-A)/2) + (1+Y) * np.log((1+A)/2)
        LOSS = np.sum(-p)
        loss = LOSS / count
        return loss
    # end def


class YNormalizationMethod(Enum):
    Nothing = 0,
    Regression = 1,
    BinaryClassifier = 2,
    MultipleClassifier = 3

class SimpleDataReader(object):
    def __init__(self, file_name):
        self.train_file_name = file_name
        self.num_train = 0
        self.XTrain = None  # normalized x, if not normalized, same as YRaw
        self.YTrain = None  # normalized y, if not normalized, same as YRaw
        self.XRaw = None    # raw x
        self.YRaw = None    # raw y

    # read data from file
    def ReadData(self):
        train_file = Path(self.train_file_name)
        if train_file.exists():
            data = np.load(self.train_file_name)
            self.XRaw = data["data"]
            self.YRaw = data["label"]
            self.num_train = self.XRaw.shape[0]
            self.XTrain = self.XRaw
            self.YTrain = self.YRaw
        else:
            raise Exception("Cannot find train file!!!")
        #end if

    def NormalizeX(self):
        X_new = np.zeros(self.XRaw.shape)
        num_feature = self.XRaw.shape[1]
        self.X_norm = np.zeros((num_feature, 2))
        # 按列归一化,即所有样本的同一特征值分别做归一化
        for i in range(num_feature):
            # get one feature from all examples
            col_i = self.XRaw[:, i]
            max_value = np.max(col_i)
            min_value = np.min(col_i)
            # min value
            self.X_norm[i, 0] = min_value
            # range value
            self.X_norm[i, 1] = max_value - min_value
            new_col = (col_i - self.X_norm[i, 0])/(self.X_norm[i, 1])
            X_new[:, i] = new_col
        #end for
        self.XTrain = X_new

    # normalize data by self range and min_value
    def NormalizePredicateData(self, X_raw):
        X_new = np.zeros(X_raw.shape)
        n = X_raw.shape[1]
        for i in range(n):
            col_i = X_raw[:, i]
            X_new[:, i] = (col_i - self.X_norm[i, 0]) / self.X_norm[i, 1]
        return X_new

    def NormalizeY(self):
        self.Y_norm = np.zeros((1, 2))
        max_value = np.max(self.YRaw)
        min_value = np.min(self.YRaw)
        # min value
        self.Y_norm[0, 0] = min_value
        # range value
        self.Y_norm[0, 1] = max_value - min_value
        y_new = (self.YRaw - min_value) / self.Y_norm[0, 1]
        self.YTrain = y_new

    # get batch training data
    def GetSingleTrainSample(self, iteration):
        x = self.XTrain[iteration]
        y = self.YTrain[iteration]
        return x, y

    # get batch training data
    def GetBatchTrainSamples(self, batch_size, iteration):
        start = iteration * batch_size
        end = start + batch_size
        batch_X = self.XTrain[start:end, :]
        batch_Y = self.YTrain[start:end, :]
        return batch_X, batch_Y

    def GetWholeTrainSamples(self):
        return self.XTrain, self.YTrain

    def Shuffle(self):
        seed = np.random.randint(0, 100)
        np.random.seed(seed)
        XP = np.random.permutation(self.XTrain)
        np.random.seed(seed)
        YP = np.random.permutation(self.YTrain)
        self.XTrain = XP
        self.YTrain = YP


class TrainingHistory(object):
    def __init__(self):
        self.iteration = []
        self.loss_history = []
        self.w_history = []
        self.b_history = []

    def AddLossHistory(self, iteration, loss):
        self.iteration.append(iteration)
        self.loss_history.append(loss)
        #self.w_history.append(w)
        #self.b_history.append(b)

    def ShowLossHistory(self, params, xmin=None, xmax=None, ymin=None, ymax=None):
        plt.plot(self.iteration, self.loss_history)
        title = params.toString()
        plt.title(title)
        plt.xlabel("iteration")
        plt.ylabel("loss")
        if xmin != None and ymin != None:
            plt.axis([xmin, xmax, ymin, ymax])
        plt.show()
        return title

    def GetLast(self):
        count = len(self.loss_history)
        return self.loss_history[count-1], self.w_history[count-1], self.b_history[count-1]

class NeuralNet(object):
    def __init__(self, params):
        self.params = params
        self.W = np.zeros((self.params.input_size, self.params.output_size))
        self.B = np.zeros((1, self.params.output_size))

    def forwardBatch(self, batch_x):
        Z = np.dot(batch_x, self.W) + self.B
        if self.params.net_type == NetType.BinaryClassifier:
            A = Sigmoid().forward(Z)
            return A
        else:
            return Z

    def backwardBatch(self, batch_x, batch_y, batch_a):
        m = batch_x.shape[0]
        dZ = batch_a - batch_y
        dB = dZ.sum(axis=0, keepdims=True)/m
        dW = np.dot(batch_x.T, dZ)/m
        return dW, dB

    def update(self, dW, dB):
        self.W = self.W - self.params.eta * dW
        self.B = self.B - self.params.eta * dB

    def inference(self, x):
        return self.forwardBatch(x)

    def train(self, dataReader, checkpoint=0.1):
        # calculate loss to decide the stop condition
        loss_history = TrainingHistory()
        loss_function = LossFunction(self.params.net_type)
        loss = 10
        if self.params.batch_size == -1:
            self.params.batch_size = dataReader.num_train
        max_iteration = math.ceil(
            dataReader.num_train / self.params.batch_size)
        checkpoint_iteration = (int)(max_iteration * checkpoint)

        for epoch in range(self.params.max_epoch):
            #print("epoch=%d" %epoch)
            dataReader.Shuffle()
            for iteration in range(max_iteration):
                # get x and y value for one sample
                batch_x, batch_y = dataReader.GetBatchTrainSamples(
                    self.params.batch_size, iteration)
                # get z from x,y
                batch_a = self.forwardBatch(batch_x)
                # calculate gradient of w and b
                dW, dB = self.backwardBatch(batch_x, batch_y, batch_a)
                # update w,b
                self.update(dW, dB)

                total_iteration = epoch * max_iteration + iteration
                if (total_iteration+1) % checkpoint_iteration == 0:
                    loss = self.checkLoss(loss_function, dataReader)
                    print(epoch, iteration, loss)
                    loss_history.AddLossHistory(
                        epoch*max_iteration+iteration, loss)
                    if loss < self.params.eps:
                        break
                    #end if
                #end if
            # end for
            if loss < self.params.eps:
                break
        # end for
        loss_history.ShowLossHistory(self.params)
        print("W=", self.W)
        print("B=", self.B)

    def checkLoss(self, loss_fun, dataReader):
        X, Y = dataReader.GetWholeTrainSamples()
        m = X.shape[0]
        A = self.forwardBatch(X)
        loss = loss_fun.CheckLoss(A, Y)
        return loss


class DataReaderEx(SimpleDataReader):
    def Add(self):
        X = self.XTrain[:, ]**2
        self.XTrain = np.hstack((self.XTrain, X))
        X = self.XTrain[:, 0:1]**3
        self.XTrain = np.hstack((self.XTrain, X))
        X = self.XTrain[:,0:1]**4
        self.XTrain = np.hstack((self.XTrain, X))
        X = self.XTrain[:, 0:1]**5
        self.XTrain = np.hstack((self.XTrain, X))


def ShowResult(net, dataReader, title):
    # draw train data
    X, Y = dataReader.XTrain, dataReader.YTrain
    plt.plot(X[:, 0], Y[:, 0], '.', c='b')
    # create and draw visualized validation data
    TX1 = np.linspace(0, 1, 100).reshape(100, 1)
    TX = np.hstack((TX1, TX1[:, ]**2))
    TX = np.hstack((TX, TX1[:, ]**3))
    TX = np.hstack((TX, TX1[:,]**4))
    TX = np.hstack((TX, TX1[:,]**5))

    TY = net.inference(TX)
    plt.plot(TX1, TY, 'x', c='r')
    plt.title(title)
    plt.show()
#end def


if __name__ == '__main__':
    dataReader = DataReaderEx(file_name)
    dataReader.ReadData()
    dataReader.Add()
    print(dataReader.XTrain.shape)

    # net
    num_input = 5
    num_output = 1
    params = HyperParameters(num_input, num_output, eta=0.2, max_epoch=10000,
                             batch_size=10, eps=0.005, net_type=NetType.Fitting)
    net = NeuralNet(params)
    net.train(dataReader, checkpoint=10)
    ShowResult(net, dataReader, params.toString())
