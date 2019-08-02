from predicator import(predicateFile)

def predicate():
  for index in range(10):
    result = predicateFile('test/' + str(index) + '.png')
    print('---------------------- begin to predicate ' +
          str(index) + ':---------------------')
    print(result)


# predicate()

result = predicateFile('./3_1.jpg')
print(result)
