# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

import numpy as np
import matplotlib.pyplot as plt
from pathlib import Path
from matplotlib.colors import LogNorm
from enum import Enum


file_name = "C:/Users/ccc/Documents/ai-edu/B-教学案例与实践/B6-神经网络基本原理简明教程/Data/ch04.npz"

class YNormalizationMethod(Enum):
    Nothing = 0,
    Regression = 1,
    BinaryClassifier = 2,
    MultipleClassifier = 3

"""
X:
    x1: feature1 feature2 feature3...
    x2: feature1 feature2 feature3...
    x3: feature1 feature2 feature3...
    ......

Y:  [if regression, value]
    [if binary classification, 0/1]
    [if multiple classification, e.g. 4 category, one-hot]

"""

class SimpleDataReader(object):
    def __init__(self):
        self.train_file_name = file_name
        self.num_train = 0
        self.XTrain = None
        self.YTrain = None

    # read data from file
    def ReadData(self):
        train_file = Path(self.train_file_name)
        print("train_file: ", train_file)
        import os
        print("abs path: ", os.path.abspath(train_file))
        if train_file.exists():
            data = np.load(self.train_file_name)
            self.XTrain = data["data"]
            self.YTrain = data["label"]
            self.num_train = self.XTrain.shape[0]
        else:
            raise Exception("Cannot find train file!!!")
        #end if

    # get batch training data
    def GetSingleTrainSample(self, iteration):
        x = self.XTrain[iteration]
        y = self.YTrain[iteration]
        return x, y

    # get batch training data
    def GetBatchTrainSamples(self, batch_size, iteration):
        start = iteration * batch_size
        end = start + batch_size
        batch_X = self.XTrain[start:end,:]
        batch_Y = self.YTrain[start:end,:]
        return batch_X, batch_Y

    def GetWholeTrainSamples(self):
        return self.XTrain, self.YTrain


    # permutation only affect along the first axis, so we need transpose the array first
    # see the comment of this class to understand the data format
    def Shuffle(self):
        seed = np.random.randint(0,100)
        np.random.seed(seed)
        XP = np.random.permutation(self.XTrain)
        np.random.seed(seed)
        YP = np.random.permutation(self.YTrain)
        self.XTrain = XP
        self.YTrain = YP

class HyperParameters(object):
    def __init__(self, input_size, output_size, eta=0.1, max_epoch=1000, batch_size=5, eps=0.1):
        self.input_size = input_size
        self.output_size = output_size
        self.eta = eta
        self.max_epoch = max_epoch
        self.batch_size = batch_size
        self.eps = eps

    def toString(self):
        title = str.format("in:{0},out:{1},bz:{2},eta:{3}", self.input_size, self.output_size, self.batch_size, self.eta)
  
class TrainingHistory(object):
    def __init__(self):
        self.iteration = []
        self.loss_history = []
        self.w_history = []
        self.b_history = []

    def AddLossHistory(self, iteration, loss, w, b):
        self.iteration.append(iteration)
        self.loss_history.append(loss)
        self.w_history.append(w)
        self.b_history.append(b)

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
# end class


class NeuralNet(object):
    def __init__(self, params):
        self.params = params
        self.W = np.zeros((self.params.input_size, self.params.output_size))
        self.B = np.zeros((1, self.params.output_size))

    def __forwardBatch(self, batch_x):
        Z = np.dot(batch_x, self.W) + self.B
        return Z

    def __backwardBatch(self, batch_x, batch_y, batch_z):
        m = batch_x.shape[0]
        dZ = batch_z - batch_y
        dB = dZ.sum(axis=0, keepdims=True)/m
        dW = np.dot(batch_x.T, dZ)/m
        return dW, dB

    def __update(self, dW, dB):
        self.W = self.W - self.params.eta * dW
        self.B = self.B - self.params.eta * dB

    def inference(self, x):
        return self.__forwardBatch(x)

    def train(self, dataReader):
        # calculate loss to decide the stop condition
        loss_history = TrainingHistory()

        if self.params.batch_size == -1:
            self.params.batch_size = dataReader.num_train
        max_iteration = (int)(dataReader.num_train / self.params.batch_size)
        for epoch in range(self.params.max_epoch):
            print("epoch=%d" %epoch)
            dataReader.Shuffle()
            for iteration in range(max_iteration):
                # get x and y value for one sample
                batch_x, batch_y = dataReader.GetBatchTrainSamples(self.params.batch_size, iteration)
                # get z from x,y
                batch_z = self.__forwardBatch(batch_x)
                # calculate gradient of w and b
                dW, dB = self.__backwardBatch(batch_x, batch_y, batch_z)
                # update w,b
                self.__update(dW, dB)
                if iteration % 2 == 0:
                    loss = self.__checkLoss(dataReader)
                    print(epoch, iteration, loss)
                    loss_history.AddLossHistory(epoch*max_iteration+iteration, loss, self.W[0,0], self.B[0,0])
                    if loss < self.params.eps:
                        break
                    #end if
                #end if
            # end for
            if loss < self.params.eps:
                break
        # end for
        loss_history.ShowLossHistory(self.params)
        print(self.W, self.B)
   
        self.loss_contour(dataReader, loss_history, self.params.batch_size, epoch*max_iteration+iteration)

    def __checkLoss(self, dataReader):
        X,Y = dataReader.GetWholeTrainSamples()
        m = X.shape[0]
        Z = self.__forwardBatch(X)
        LOSS = (Z - Y)**2
        loss = LOSS.sum()/m/2
        return loss

    def loss_contour(self, dataReader,loss_history,batch_size,iteration):
        last_loss, result_w, result_b = loss_history.GetLast()
        X,Y=dataReader.GetWholeTrainSamples()
        len1 = 50
        len2 = 50
        w = np.linspace(result_w-1,result_w+1,len1)
        b = np.linspace(result_b-1,result_b+1,len2)
        W,B = np.meshgrid(w,b)
        len = len1 * len2
        X,Y = dataReader.GetWholeTrainSamples()
        m = X.shape[0]
        Z = np.dot(X, W.ravel().reshape(1,len)) + B.ravel().reshape(1,len)
        Loss1 = (Z - Y)**2
        Loss2 = Loss1.sum(axis=0,keepdims=True)/m
        Loss3 = Loss2.reshape(len1, len2)
        plt.contour(W,B,Loss3,levels=np.logspace(-5, 5, 100), norm=LogNorm(), cmap=plt.cm.jet)

        # show w,b trace
        w_history = loss_history.w_history
        b_history = loss_history.b_history
        plt.plot(w_history,b_history)
        plt.xlabel("w")
        plt.ylabel("b")
        title = str.format("batchsize={0}, iteration={1}, w={2:.3f}, b={3:.3f}", batch_size, iteration, result_w, result_b)
        plt.title(title)

        plt.axis([result_w-1,result_w+1,result_b-1,result_b+1])
        plt.show()
        
        return title

if __name__ == '__main__':
    sdr = SimpleDataReader()
    sdr.ReadData()
    params = HyperParameters(1, 1, eta=0.1, max_epoch=1000, batch_size=-1, eps = 0.02)
    net = NeuralNet(params)
    net.train(sdr)
