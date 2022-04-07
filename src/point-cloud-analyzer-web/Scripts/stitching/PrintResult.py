import csv
import numpy as np
import os

#########找最新的檔案
def find_new_file(dir):
     '''查詢目錄下最新的檔案'''
     file_lists = os.listdir(dir)
     # file_lists.sort(key=lambda fn: os.path.getmtime(dir + "\\" + fn)
     #                 if not os.path.isdir(dir + "\\" + fn) else 0)
     file_lists.sort(key=lambda fn: os.path.getmtime(dir + fn)
                     if not os.path.isdir(dir + fn) else 0)
     print('最新的檔案為： ' + file_lists[-1])
     file = os.path.join(dir, file_lists[-1])
     return file_lists[-1]









def RequestHead(P, SAI, RAI):  ##用已取得輸出資料的欄位名稱
    oname = ['評估函數', '平均電量耗費', '剩餘總電量', '最大TID', '最大T剩餘電量', '平均T剩餘電量', '最小TID', '最小T剩餘電量']
    if RAI is None:
        name = ['round'] + SAI.Fname + oname
    else:
        name = ['round'] + SAI.Fname + RAI.Fname + oname
    return name


def RequestAllFitness(P, SAI, RAI, s):  ##取得所有fitness欄位
    if RAI is None:
        return SAI.Evaluate(P, s)
    else:
        return list(SAI.Evaluate(P, s)) + list(RAI.Evaluate(P, s))


# --------------產生統計報告（第一次時使用）---------
def CreateReport(P, report, SAI, RAI=None):
    name = RequestHead(P, SAI, RAI)
    file = open(report, "w", newline="")
    for i in range(P.SENSOR_NUMBER):
        name.append('Sch' + str(i))
    for i in range(P.SENSOR_NUMBER):
        name.append('Rou' + str(i))
    file = csv.writer(file)
    file.writerow(name)
    return file


# --------------輸出此次決策出來的統計報告----------
# P : 測試資料問題
# s : 決策出的State
# cost : 決策的耗費電量
# i : 這是第幾round
# SAI、RAI使用的排程與路由方法 （如果混合RAI＝＝NONE）
# test : DEBUG 輸出
# report : 是否要寫入到data的統計資料中
def PrintProblemStatus(P, s, cost, i, SAI, RAI=None, test=False, report=None):
    name = RequestHead(P, SAI, RAI)
    value = [i]
    F = RequestAllFitness(P, SAI, RAI, s)
    for f in F:  ##根據fitness輸入到統計資料
        value.append(f)
    target_red = P.CalTargetRed(P.J - cost)  ##計算其他的統計資料
    value = value + [sum(F), np.mean(cost), np.sum(P.J - cost), np.argmax(target_red), np.max(target_red),
                     np.mean(target_red), np.argmin(target_red), np.min(target_red)]

    if test:  ##DEBUG用
        for i in range(len(value)):
            if '電量' in name[i]:
                v = str(value[i]) + 'mj'
            else:
                v = value[i]
            print(name[i], ':', v)
        print('最大轉送封包之SID、TID、轉送量、轉送距離')

        print('------------------------------')
    if report is not None:  ##表示此次有決策出sch和rou，要寫入統計資料.csv中

        value = value + s.sch.tolist() + s.rou.tolist()
        report.writerow(value)