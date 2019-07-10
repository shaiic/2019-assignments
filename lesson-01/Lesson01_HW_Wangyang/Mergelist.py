def listMerge(lst1,lst2):
    lst = []
    i=0
    j=0

    while(i < len(lst1) and j < len(lst2)):
        if lst1[i] <= lst2[j]:
            lst.append(lst1[i])
            if i < len(lst1) -1:
                i+=1
            else:
                lst += lst2[j,len(lst2)]
                break
        else:
            lst.append(lst2[j])
            if j < len(lst2) -1:
                j+=1
            else:
                lst += lst1[i:len(lst1)]
                break
    return lst


def main():
    lst1=[1,3,4,7,8,9]
    lst2=[2,3,5,6]
    lst = listMerge(lst1,lst2)
    print(lst)


if __name__ == '__main__':
    main()
