# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

import numpy as np
import csv
import copy
import pandas as pd
import matplotlib.pyplot as plt
import scipy.stats as stats

data_file_name = "./Data/kc_house_data.csv"     # download from Kc
data_file = "./Data/ch14.house.data.npz" #训练集与测试集分出来之前的全部数据
train_file = "./Data/ch14.house.train.npz"
test_file = "./Data/ch14.house.test.npz"

class HouseSingleDataProcessor(object):
    #读取数据
    def PrepareData(self, csv_file):
        self.X = None
        self.Y = None
        i = 0
        with open(csv_file, newline='') as f:
            reader = csv.reader(f)
            array_x = np.zeros((1,13))
            array_y = np.zeros((1,1))
            for row in reader:
                if i == 0:
                    i += 1
                    continue
                # don't need to read 'No' and 'Year'
                #row[0] id：唯一id 没有用
                #row[1] date：售出日期 没有用
                #row[2] price：售出价格（标签值） 不在这里取
                array_x[0,0] = row[3]   # bedrooms  卧室数量
                array_x[0,1] = row[4]   # bathrooms 浴室数量
                array_x[0,2] = row[5]   # sqft living 居住面积
                array_x[0,3] = row[6]   # sqft lot 停车场面积
                array_x[0,4] = row[7]   # floors 楼层数
                array_x[0,5] = row[8]   # waterfront 泳池
                #row[9] view：有多少次看房记录 没有用
                array_x[0,6] = row[10]   # condition 房屋状况
                array_x[0,7] = row[11]  # grade 评级
                array_x[0,8] = row[12]  # sqft above 地面上的面积
                array_x[0,9] = row[13]  # sqft base 地下室的面积
                array_x[0,10] = row[14]  # year built 建筑年份
                if (int)(row[15]) != 0:
                    array_x[0,10] = (int)(row[15]) #yr_renovated：翻修年份
                #row[16] #zipcode：邮政编码 没有用
                #array_x[0,11] = row[15]  # year renovate
                array_x[0,11] = row[17]  # latitude 维度
                array_x[0,12] = row[18]  # longitude 经度
                if (float)(row[19]) != 0:
                    array_x[0,2] = (float)(row[19]) #row[19] sqft_living15：2015年翻修后的居住面积
                if (float)(row[20]) != 0:
                    array_x[0,3] = (float)(row[20]) #row[20] sqft_lot15：2015年翻修后的停车场面积

                #取标签值
                array_y[0,0] = (float)(row[2])   # label 

                if self.X is None:
                    self.X = array_x.copy()
                else:
                    self.X = np.vstack((self.X, array_x))
                #end if
                if self.Y is None:
                    self.Y = array_y.copy()
                    print(self.Y.shape)
                else:
                    self.Y = np.vstack((self.Y, array_y))
                #end if

                i = i+1
                if i % 100 == 0:
                    print(i)
            #end for
        #end with    
    #把解析出来的数据保存下来  
    def saveData(self):
        np.savez(data_file, data=self.X, label=self.Y)
#查看数据分布
def watchDistribution():
    data = np.load(data_file)
    XRaw=data["data"]
    YRaw=data["label"]
    #对数据进行观察
    df = pd.DataFrame({
        "bedrooms"      : XRaw[:, 0],#卧室数量
        "bathrooms"     : XRaw[:, 1],#浴室数量
        "sqft living"   : XRaw[:, 2],#居住面积
        "sqft lot"      : XRaw[:, 3],#停车场面积
        "floors"        : XRaw[:, 4],#楼层数
        "waterfront"    : XRaw[:, 5],#泳池
        "condition"     : XRaw[:, 6],#房屋状况
        "grade"         : XRaw[:, 7],#评级
        "sqft above"    : XRaw[:, 8],#地面上的面积
        "sqft base"     : XRaw[:, 9],#地下室的面积
        "year built"    : XRaw[:,10],#建筑年份
        "latitude"      : XRaw[:,11],#维度
        "longitude"     : XRaw[:,12],#经度
        "price"         : YRaw[:,0]  #标签值
    })
    print(df["bedrooms"].describe())
    print(df["bathrooms"].describe())
    #直方图、均值、中值、分布密度的方式观察数据、
    plt.figure()
    #bedrooms
    axes = plt.subplot(2,7,1)
    axes.set_title("bedrooms")
    n,x,_=axes.hist(df["bedrooms"],histtype='step', density=True)
    axes.axvline(df["bedrooms"].mean(), color='magenta', linestyle='dashed', linewidth=2)
    axes.axvline(df["bedrooms"].median(), color='green', linestyle='dashed', linewidth=2)    
    density = stats.gaussian_kde(df["bedrooms"])
    axes.plot(x, density(x)*2)
    #bathrooms
    axes = plt.subplot(2,7,2)
    axes.set_title("bathrooms")
    n,x,_=axes.hist(df["bathrooms"],histtype='step', density=True)
    axes.axvline(df["bathrooms"].mean(), color='magenta', linestyle='dashed', linewidth=2)
    axes.axvline(df["bathrooms"].median(), color='green', linestyle='dashed', linewidth=2)
    density = stats.gaussian_kde(df["bathrooms"])
    axes.plot(x, density(x)*0.6)
    #sqft living
    axes = plt.subplot(2,7,3)
    axes.set_title("sqft living")
    n,x,_=axes.hist(df["sqft living"],histtype='step', density=True)
    axes.axvline(df["sqft living"].mean(), color='magenta', linestyle='dashed', linewidth=2)
    axes.axvline(df["sqft living"].median(), color='green', linestyle='dashed', linewidth=2)
    density = stats.gaussian_kde(df["sqft living"])
    axes.plot(x, density(x)*0.8)
    #sqft lot
    axes = plt.subplot(2,7,4)
    axes.set_title("sqft lot")
    n,x,_=axes.hist(df["sqft lot"],histtype='step', density=True)
    axes.axvline(df["sqft lot"].mean(), color='magenta', linestyle='dashed', linewidth=2)
    axes.axvline(df["sqft lot"].median(), color='green', linestyle='dashed', linewidth=2)
    density = stats.gaussian_kde(df["sqft lot"])
    axes.plot(x, density(x)*0.4)
    #floors
    axes = plt.subplot(2,7,5)
    axes.set_title("floors")
    n,x,_=axes.hist(df["floors"],histtype='step', density=True)
    axes.axvline(df["floors"].mean(), color='magenta', linestyle='dashed', linewidth=2)
    axes.axvline(df["floors"].median(), color='green', linestyle='dashed', linewidth=2)
    density = stats.gaussian_kde(df["floors"])
    axes.plot(x, density(x)*0.7)
    #waterfront
    axes = plt.subplot(2,7,6)
    axes.set_title("waterfront")
    n,x,_=axes.hist(df["waterfront"],histtype='step', density=True)
    axes.axvline(df["waterfront"].mean(), color='magenta', linestyle='dashed', linewidth=2)
    axes.axvline(df["waterfront"].median(), color='green', linestyle='dashed', linewidth=2)
    density = stats.gaussian_kde(df["waterfront"])
    axes.plot(x, density(x)*0.3)
    #condition
    axes = plt.subplot(2,7,7)
    axes.set_title("condition")
    n,x,_=axes.hist(df["condition"],histtype='step', density=True)
    axes.axvline(df["condition"].mean(), color='magenta', linestyle='dashed', linewidth=2)
    axes.axvline(df["condition"].median(), color='green', linestyle='dashed', linewidth=2)
    density = stats.gaussian_kde(df["condition"])
    axes.plot(x, density(x)*0.5)
    #grade
    axes = plt.subplot(2,7,8)
    axes.set_title("grade")
    n,x,_=axes.hist(df["grade"],histtype='step', density=True)
    axes.axvline(df["grade"].mean(), color='magenta', linestyle='dashed', linewidth=2)
    axes.axvline(df["grade"].median(), color='green', linestyle='dashed', linewidth=2)
    density = stats.gaussian_kde(df["grade"])
    axes.plot(x, density(x)*0.5)
    #sqft above
    axes = plt.subplot(2,7,9)
    axes.set_title("sqft above")
    n,x,_=axes.hist(df["sqft above"],histtype='step', density=True)
    axes.axvline(df["sqft above"].mean(), color='magenta', linestyle='dashed', linewidth=2)
    axes.axvline(df["sqft above"].median(), color='green', linestyle='dashed', linewidth=2)
    density = stats.gaussian_kde(df["sqft above"])
    axes.plot(x, density(x)*0.8)
    #sqft base
    axes = plt.subplot(2,7,10)
    axes.set_title("sqft base")
    n,x,_=axes.hist(df["sqft base"],histtype='step', density=True)
    axes.axvline(df["sqft base"].mean(), color='magenta', linestyle='dashed', linewidth=2)
    axes.axvline(df["sqft base"].median(), color='green', linestyle='dashed', linewidth=2)
    density = stats.gaussian_kde(df["sqft base"])
    axes.plot(x, density(x)*0.4)
    #year built
    axes = plt.subplot(2,7,11)
    axes.set_title("year built")
    n,x,_=axes.hist(df["year built"],histtype='step', density=True)
    axes.axvline(df["year built"].mean(), color='magenta', linestyle='dashed', linewidth=2)
    axes.axvline(df["year built"].median(), color='green', linestyle='dashed', linewidth=2)
    density = stats.gaussian_kde(df["year built"])
    axes.plot(x, density(x)*0.9)
    #latitude
    axes = plt.subplot(2,7,12)
    axes.set_title("latitude")
    n,x,_=axes.hist(df["latitude"],histtype='step', density=True)
    axes.axvline(df["latitude"].mean(), color='magenta', linestyle='dashed', linewidth=2)
    axes.axvline(df["latitude"].median(), color='green', linestyle='dashed', linewidth=2)
    density = stats.gaussian_kde(df["latitude"])
    axes.plot(x, density(x))
    #longitude
    axes = plt.subplot(2,7,13)
    axes.set_title("longitude")
    n,x,_=axes.hist(df["longitude"],histtype='step', density=True)
    axes.axvline(df["longitude"].mean(), color='magenta', linestyle='dashed', linewidth=2)
    axes.axvline(df["longitude"].median(), color='green', linestyle='dashed', linewidth=2)
    density = stats.gaussian_kde(df["longitude"])
    axes.plot(x, density(x)*1.2)
    #price
    axes = plt.subplot(2,7,14)
    axes.set_title("price")
    n,x,_=axes.hist(df["price"],histtype='step', density=True)
    axes.axvline(df["price"].mean(), color='magenta', linestyle='dashed', linewidth=2)
    axes.axvline(df["price"].median(), color='green', linestyle='dashed', linewidth=2)
    density = stats.gaussian_kde(df["price"])
    axes.plot(x, density(x)*2.5)


    plt.show()
    #dr.RandomSplit(dr.X, dr.Y, 0.2)
#查看四分位数
def watchQuantile():
    data = np.load(data_file)
    XRaw=data["data"]
    YRaw=data["label"]
    #对数据进行观察
    df = pd.DataFrame({
        "bedrooms"      : XRaw[:, 0],#卧室数量
        "bathrooms"     : XRaw[:, 1],#浴室数量
        "sqft living"   : XRaw[:, 2],#居住面积
        "sqft lot"      : XRaw[:, 3],#停车场面积
        "floors"        : XRaw[:, 4],#楼层数
        "waterfront"    : XRaw[:, 5],#泳池
        "condition"     : XRaw[:, 6],#房屋状况
        "grade"         : XRaw[:, 7],#评级
        "sqft above"    : XRaw[:, 8],#地面上的面积
        "sqft base"     : XRaw[:, 9],#地下室的面积
        "year built"    : XRaw[:,10],#建筑年份
        "latitude"      : XRaw[:,11],#维度
        "longitude"     : XRaw[:,12],#经度
        "price"         : YRaw[:,0]  #标签值
    })
    plt.figure()
    #bedrooms
    axes = plt.subplot(2,7,1)
    axes.set_title("bedrooms")
    df["bedrooms"].plot(kind="box",ax=axes)    
    #bathrooms
    axes = plt.subplot(2,7,2)
    axes.set_title("bathrooms")
    df["bathrooms"].plot(kind="box",ax=axes) 
    #sqft living
    axes = plt.subplot(2,7,3)
    axes.set_title("sqft living")
    df["sqft living"].plot(kind="box",ax=axes) 
    #sqft lot
    axes = plt.subplot(2,7,4)
    axes.set_title("sqft lot")
    df["sqft lot"].plot(kind="box",ax=axes)
    #floors
    axes = plt.subplot(2,7,5)
    axes.set_title("floors")
    df["floors"].plot(kind="box",ax=axes)
    #waterfront
    axes = plt.subplot(2,7,6)
    axes.set_title("waterfront")
    df["waterfront"].plot(kind="box",ax=axes)
    #condition
    axes = plt.subplot(2,7,7)
    axes.set_title("condition")
    df["condition"].plot(kind="box",ax=axes)
    #grade
    axes = plt.subplot(2,7,8)
    axes.set_title("grade")
    df["grade"].plot(kind="box",ax=axes)
    #sqft above
    axes = plt.subplot(2,7,9)
    axes.set_title("sqft above")
    df["sqft above"].plot(kind="box",ax=axes)
    #sqft base
    axes = plt.subplot(2,7,10)
    axes.set_title("sqft base")
    df["sqft base"].plot(kind="box",ax=axes)
    #year built
    axes = plt.subplot(2,7,11)
    axes.set_title("year built")
    df["year built"].plot(kind="box",ax=axes)
    #latitude
    axes = plt.subplot(2,7,12)
    axes.set_title("latitude")
    df["latitude"].plot(kind="box",ax=axes)
    #longitude
    axes = plt.subplot(2,7,13)
    axes.set_title("longitude")
    df["longitude"].plot(kind="box",ax=axes)
    #price
    axes = plt.subplot(2,7,14)
    axes.set_title("price")
    df["price"].plot(kind="box",ax=axes)


    plt.show()
#查看散点图
def watchScatter():
    data = np.load(data_file)
    XRaw=data["data"]
    YRaw=data["label"]
    #对数据进行观察
    df = pd.DataFrame({
        "bedrooms"      : XRaw[:, 0],#卧室数量
        "bathrooms"     : XRaw[:, 1],#浴室数量
        "sqft living"   : XRaw[:, 2],#居住面积
        "sqft lot"      : XRaw[:, 3],#停车场面积
        "floors"        : XRaw[:, 4],#楼层数
        "waterfront"    : XRaw[:, 5],#泳池
        "condition"     : XRaw[:, 6],#房屋状况
        "grade"         : XRaw[:, 7],#评级
        "sqft above"    : XRaw[:, 8],#地面上的面积
        "sqft base"     : XRaw[:, 9],#地下室的面积
        "year built"    : XRaw[:,10],#建筑年份
        "latitude"      : XRaw[:,11],#维度
        "longitude"     : XRaw[:,12],#经度
        "price"         : YRaw[:,0]  #标签值
    })
    plt.figure()
    #bedrooms
    axes = plt.subplot(2,7,1)
    axes.set_title("bedrooms")
    axes.scatter(df["bedrooms"],df["price"])
    #bathrooms
    axes = plt.subplot(2,7,2)
    axes.set_title("bathrooms")
    axes.scatter(df["bathrooms"],df["price"])
    #sqft living
    axes = plt.subplot(2,7,3)
    axes.set_title("sqft living")
    axes.scatter(df["sqft living"],df["price"])
    #sqft lot
    axes = plt.subplot(2,7,4)
    axes.set_title("sqft lot")
    axes.scatter(df["sqft lot"],df["price"])
    #floors
    axes = plt.subplot(2,7,5)
    axes.set_title("floors")
    axes.scatter(df["floors"],df["price"])
    #waterfront
    axes = plt.subplot(2,7,6)
    axes.set_title("waterfront")
    axes.scatter(df["waterfront"],df["price"])
    #condition
    axes = plt.subplot(2,7,7)
    axes.set_title("condition")
    axes.scatter(df["condition"],df["price"])
    #grade
    axes = plt.subplot(2,7,8)
    axes.set_title("grade")
    axes.scatter(df["grade"],df["price"])
    #sqft above
    axes = plt.subplot(2,7,9)
    axes.set_title("sqft above")
    axes.scatter(df["sqft above"],df["price"])
    #sqft base
    axes = plt.subplot(2,7,10)
    axes.set_title("sqft base")
    axes.scatter(df["sqft base"],df["price"])
    #year built
    axes = plt.subplot(2,7,11)
    axes.set_title("year built")
    axes.scatter(df["year built"],df["price"])
    #latitude
    axes = plt.subplot(2,7,12)
    axes.set_title("latitude")
    axes.scatter(df["latitude"],df["price"])
    #longitude
    axes = plt.subplot(2,7,13)
    axes.set_title("longitude")
    axes.scatter(df["longitude"],df["price"])

    plt.show()
#按四比1的比例分离出训练集和测试集                
def RandomSplit(ratio=0.2):
    data = np.load(data_file)
    XRaw=data["data"]
    YRaw=data["label"]
    #assert(x.shape[0] == y.shape[0])
    total = XRaw.shape[0]
    seed = np.random.randint(0,100)
    np.random.seed(seed)
    np.random.shuffle(XRaw)
    np.random.seed(seed)
    np.random.shuffle(YRaw)

    testcount = (int)(total * ratio)
    
    x_test = XRaw[0:testcount,:]
    x_train = XRaw[testcount:,:]
    y_test = YRaw[0:testcount,:]
    y_train = YRaw[testcount:,:]

    np.savez(train_file, data=x_train, label=y_train)
    np.savez(test_file, data=x_test, label=y_test)
    
    print(x_train.shape, y_train.shape, x_test.shape, y_test.shape)
if __name__ == '__main__':
    #dr = HouseSingleDataProcessor()
    #dr.PrepareData(data_file_name)
    #dr.saveData()#保存数据    
    #watchDistribution()#查看数据分布
    #watchQuantile()#查看四分位数
    #查看各个属性与最终价格的散点图
    #watchScatter()
    RandomSplit()