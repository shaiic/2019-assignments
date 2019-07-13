## 环境配置

### 获取代码

```bash
git clone https://github.com/shaiic/2019-assignments.git
cd lesson-07
```
解压`lesson-07`文件夹下的zip包：`assignment1.zip`,生成一个名为`assignment1`的文件夹。

### 获取数据

Once you have the starter code (regardless of which method you choose above), you will need to download the CIFAR-10 dataset. Run the following from the `assignment1` directory:

```
cd assignments1/cs231n/datasets
./get_datasets.sh
```

### 安装依赖包

```bash
cd assignment1
pip install -r requirements.txt  # Install dependencies
```

### 创建工作目录

完成上述环境部署之后，将`assignment1`文件夹改为你自己的名字（英文名）命名的文件，就可以开始完成后续作业了。

### Start IPython:

you should start the IPython notebook server from the `assignment1` directory(替换为你自己的工作目录), with the `jupyter notebook` command.
```bash
cd assignment1
jupyter notebook
```



## 开始编写代码

在jupiter notebook中打开文件`two_layer_net.ipynb`，按照引导开始编写两层神经网络。



### 扩展

如果学有余力，可以进一步的挑战cs231n assignment02中的多层神经网络：http://cs231n.github.io/assignments2019/assignment2/
```
Q1: Fully-connected Neural Network (20 points)

The IPython notebook FullyConnectedNets.ipynb will introduce you to our modular layer design, and then use those layers to implement fully-connected networks of arbitrary depth. To optimize these models you will implement several popular update rules.
```