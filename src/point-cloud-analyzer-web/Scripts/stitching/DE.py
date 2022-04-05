import numpy as np
import random
import time
import math
import copy
import open3d  as o3d
from scipy.spatial import distance
# class DE():  # DE的模板
#   def __init__(self, n=50, l=21, cu=0.9, mu=0.8):
#     self.N = n
#     self.cu = cu
#     self.mu = mu
#     self.name = self.__class__.__name__ + '_' + str(n) + str(self.mu) + str(self.cu)
#     self.path = None  ##紀錄一次run的每個iteration的結果在哪
#     self.history = []
#     self.length = l
#     self.evatime = 0

def calFitness(X,P,Q):
    ## 計算出轉換矩陣
    T = np.zeros((4, 4))
    temp = np.zeros((1, 3))
    temp[0, 0] = X[0] * np.pi * (1 / 180) # 角度轉換為弧度
    temp[0, 1] = X[1] * np.pi * (1 / 180)  # 角度轉換為弧度
    temp[0, 2] = X[2] * np.pi * (1 / 180)  # 角度轉換為弧度
    mesh = o3d.geometry.TriangleMesh.create_coordinate_frame()
    R = mesh.get_rotation_matrix_from_xyz(temp[0])
    T[0:3, 0:3] = R

    up = P.mean(axis=0)
    uq = Q.mean(axis=0)
    t = uq - np.dot(R, up)
    T[0:3, 3] = t
    T[3, 3] = 1.0

    ##依照轉換矩陣將 P 旋轉
    trans_P = P.copy()
    trans_P = np.dot(trans_P, R.T) - t
    #0105-#######計算fitness
    assert len(trans_P) == len(Q)
    error = 0

    for i in range (len(trans_P)):
        error+= distance.euclidean(trans_P[i], Q[i])
    # error = np.linalg.norm(
    #     Q - np.dot(P, R.T) - t,
    #     axis=1
    # )
    error /= len(trans_P) ## RMSE = sigma(|P-Q|2)/N
    #print(error)
    return -error


def mutation(XTemp, F):
    m, n = np.shape(XTemp) ##m = num of chromosome , n = 3
    #print(m,n)
    XMutationTmp = np.zeros((m, n))
    for i in range(m):
        r1 = 0
        r2 = 0
        r3 = 0
        while r1 == i or r2 == i or r3 == i or r1 == r2 or r1 == r3 or r2 == r3:
            r1 = random.randint(0, m - 1)
            r2 = random.randint(0, m - 1)
            r3 = random.randint(0, m - 1)

        for j in range(n):
            XMutationTmp[i, j] = XTemp[r1, j] + F * (XTemp[r2, j] - XTemp[r3, j])

    return XMutationTmp

def crossover(XTemp, XMutationTmp, CR):
    m, n = np.shape(XTemp)
    XCorssOverTmp = np.zeros((m,n))
    for i in range(m):
        for j in range(n):
            r = random.random()
            if (r <= CR):
                XCorssOverTmp[i,j] = XMutationTmp[i,j]
            else:
                XCorssOverTmp[i,j] = XTemp[i,j]
    return XCorssOverTmp

def selection(XTemp, XCorssOverTmp, fitnessVal, P, Q):
    m,n = np.shape(XTemp)
    fitnessCrossOverVal = np.zeros((m,1))
    for i in range(m):
        fitnessCrossOverVal[i,0] = calFitness(XCorssOverTmp[i], P, Q)
        #print(fitnessCrossOverVal[i,0])
        if (fitnessCrossOverVal[i,0] > fitnessVal[i,0]): ## Fitness 越大越好
            for j in range(n):
                XTemp[i,j] = XCorssOverTmp[i,j]
            fitnessVal[i,0] = fitnessCrossOverVal[i,0]
    return XTemp, fitnessVal

def solve_de_transf(P,Q):
    # P, Q: 依照對應關系排序好的點雲
    ## Step1 : Parameters defination
    NP = 10
    dim = 3  #dimention
    xMin = -180
    xMax = 180
    F = 0.5
    CR = 0.8
    iter = 10


    ## Step2 : Init Population
    XTemp = np.zeros((NP, 3))
    for i in range(NP):
        for j in range(3):
            XTemp[i, j] = xMin + random.random() * (xMax - xMin)
        #print(XTemp[i])

    ## Step3 : Cal Fitness -每個染色體
    fitnessVal = np.zeros((NP, 1))
    for i in range(NP):
        #print("chromosome = ",XTemp[i])
        fitnessVal[i, 0] = calFitness(XTemp[i], P, Q)
    #print(fitnessVal)

    it = 0
    while it < iter :
        ## Step4 : Mutation
        XMutationTmp = mutation(XTemp, F)
        #print(XMutationTmp)
        ## Step5 : Crossover
        XCorssOverTmp = crossover(XTemp, XMutationTmp, CR)
        #rint(XCorssOverTmp)
        ## Step6 : Selection
        XTemp, fitnessVal = selection(XTemp, XCorssOverTmp, fitnessVal, P, Q)
        #print("=======selected=========")
        #print(XTemp)
        ################################################################
        #1/11##########
        best_id = np.argmax(fitnessVal)
        best_x = XTemp[best_id]
        ## 計算出轉換矩陣
        T = np.zeros((4, 4))
        temp = np.zeros((1, 3))
        temp[0, 0] = best_x[0] * np.pi * (1 / 180)  # 角度轉換為弧度
        temp[0, 1] = best_x[1] * np.pi * (1 / 180)  # 角度轉換為弧度
        temp[0, 2] = best_x[2] * np.pi * (1 / 180)  # 角度轉換為弧度
        mesh = o3d.geometry.TriangleMesh.create_coordinate_frame()
        R = mesh.get_rotation_matrix_from_xyz(temp[0])
        T[0:3, 0:3] = R

        up = P.mean(axis=0)
        uq = Q.mean(axis=0)
        t = uq - np.dot(R, up)
        T[0:3, 3] = t
        T[3, 3] = 1.0
        #############################################################
        it += 1
    return T
#solve_de_transf()