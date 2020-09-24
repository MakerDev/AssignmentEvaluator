W = int(input('무게:'))
H = int(input('높이:'))
K = int(input('온도:'))


print(W ,'kg','=>', format(W/0.45359,'.1f') ,'pounds')
print(H ,'cm','=>', format(H/30.48,'.1f') ,'feet')
print(K ,'C','=>', format(K*9/5+32,'.1f')  ,'F')
