import numpy as np
import matplotlib.pyplot as plt
from pathlib import Path
from matplotlib.colors import LogNorm


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
        return title


class SimpleDataReader(object):
    def __init__(self):
        self.train_file_name = ""
        self.num_train = 0
        self.XTrain = None
        self.YTrain = None


    # read data from file
    def ReadData(self):
        train_file = Path(self.train_file_name)
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

class NeuralNet(object):
    def __init__(self, params):
        self.params = params
        self.w = np.zeros((self.params.input_size, self.params.output_size))
        self.b = np.zeros((1, self.params.output_size))

    def __forwardBatch(self, batch_x):
        Z = np.dot(batch_x, self.w) + self.b
        return Z

    def __backwardBatch(self, batch_x, batch_y, batch_z):
        m = batch_x.shape[0]
        dZ = batch_z - batch_y
        dB = dZ.sum(axis=0, keepdims=True) / m
        dW = np.dot(batch_x.T, dZ) / m
        return dW, dB

    def __update(self, dW, dB):
        self.w = self.w - self.params.eta * dW
        self.b = self.b - self.params.eta * dB

    def inference(self, x):
        return self.__forwardBatch(x)

    def train(self, dataReader):
        # calculate loss to decide the stop condition
        loss_history = TrainingHistory()

        if self.params.batch_size == -1:
            self.params.batch_size = dataReader.num_train
        max_iteration = (int)(dataReader.num_train / self.params.batch_size)
        for epoch in range(self.params.max_epoch):
            print("epoch=%d" % epoch)
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
                    loss_history.AddLossHistory(epoch * max_iteration + iteration, loss, self.w[0, 0], self.b[0, 0])
                    if loss < self.params.eps:
                        break
                    # end if
                # end if
            # end for
            if loss < self.params.eps:
                break
        # end for
        loss_history.ShowLossHistory(self.params)
        print(self.w, self.b)

        self.loss_contour(dataReader, loss_history, self.params.batch_size, epoch * max_iteration + iteration)

    def __checkLoss(self, dataReader):
        X, Y = dataReader.GetWholeTrainSamples()
        m = X.shape[0]
        Z = self.__forwardBatch(X)
        LOSS = (Z - Y) ** 2
        loss = LOSS.sum() / m / 2
        return loss

    def loss_contour(self, dataReader, loss_history, batch_size, iteration):
        last_loss, result_w, result_b = loss_history.GetLast()
        X, Y = dataReader.GetWholeTrainSamples()
        len1 = 50
        len2 = 50
        w = np.linspace(result_w - 1, result_w + 1, len1)
        b = np.linspace(result_b - 1, result_b + 1, len2)
        W, B = np.meshgrid(w, b)
        len = len1 * len2
        X, Y = dataReader.GetWholeTrainSamples()
        m = X.shape[0]
        Z = np.dot(X, W.ravel().reshape(1, len)) + B.ravel().reshape(1, len)
        Loss1 = (Z - Y) ** 2
        Loss2 = Loss1.sum(axis=0, keepdims=True) / m
        Loss3 = Loss2.reshape(len1, len2)
        plt.contour(W, B, Loss3, levels=np.logspace(-5, 5, 100), norm=LogNorm(), cmap=plt.cm.jet)

        # show w,b trace
        w_history = loss_history.w_history
        b_history = loss_history.b_history
        plt.plot(w_history, b_history)
        plt.xlabel("w")
        plt.ylabel("b")
        title = str.format("batchsize={0}, iteration={1}, w={2:.3f}, b={3:.3f}", batch_size, iteration, result_w,
                           result_b)
        plt.title(title)

        plt.axis([result_w - 1, result_w + 1, result_b - 1, result_b + 1])
        plt.show()


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

class LogicNotGateDataReader(SimpleDataReader):
    # x=0,y=1; x=1,y=0
    def ReadData(self):
        X = np.array([0,1]).reshape(2,1)
        Y = np.array([1,0]).reshape(2,1)
        self.XTrain = X
        self.YTrain = Y
        self.num_train = 2

def Test(net):
    z1 = net.inference(0)
    z2 = net.inference(1)
    print (z1,z2)
    if np.abs(z1-1) < 0.001 and np.abs(z2-0)<0.001:
        return True
    return False

def ShowResult(net):
    x = np.array([-0.5,0,1,1.5]).reshape(4,1)
    y = net.inference(x)
    plt.plot(x,y)
    plt.scatter(0,1,marker='^')
    plt.scatter(1,0,marker='o')
    plt.show()

if __name__ == '__main__':
     # read data
    sdr = LogicNotGateDataReader()
    sdr.ReadData()
    # create net
    params = HyperParameters(1, 1, eta=0.1, max_epoch=1000, batch_size=1, eps = 1e-8)
    net = NeuralNet(params)
    net.train(sdr)
    # result
    print("w=%f,b=%f" %(net.W, net.B))
    # predication
    print(Test(net))
    ShowResult(net)
