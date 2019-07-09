import random

def findnum1(lst,lsum):
    return lsum-sum(lst)

def findnum2(lst,length):
    keylist = [0] * length
    for num in lst:
        keylist[num] +=1

    keynum = 0
    for i in range(length):
        if keylist[i] == 0:
            keynum = i
            break

    return keynum

def main():
    #初始化列表
    lnum = list(range(0,101))
    lsum = sum(lnum)
    llen = len(lnum)

    #乱序列表并删除随机值
    keynum = random.randint(0,100)
    lnum[keynum] = 0
    random.shuffle(lnum)

    key1 = findnum1(lnum,lsum)
    key2 = findnum2(lnum,llen)

    print("The random number is:" + str(keynum))
    print("The number finded by method 1 is:" + str(key1))
    print("The number finded by method 2 is:" + str(key2))

if __name__ == "__main__":
    main()
