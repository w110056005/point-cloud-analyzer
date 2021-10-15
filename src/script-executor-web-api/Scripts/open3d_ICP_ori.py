import open3d as o3d 
import numpy as np
import math
import copy
import sys


# read point cloud data
voxel_size = 0.02
pcds = [] # save pcd into list
source = o3d.io.read_point_cloud(sys.argv[1])
source.paint_uniform_color([1, 0.706, 0]) # source is yello
pcds.append(source)
target = o3d.io.read_point_cloud(sys.argv[2])
target.paint_uniform_color([0, 0.651, 0.929]) # target is blue
pcds.append(target)

#o3d.visualization.draw_geometries([source,target], window_name  ='Open3D Original', width=1080, height=720)

threshold = 0.02
trans_init = np.asarray([[1, 0, 0, 0],
                         [0, 1, 0, 0],
                         [0, 0, 1, 0],
                         [0, 0, 0, 1]])

#
# # Run ICP
# print("Apply point-to-point ICP")
# def ICP_test(ply1, ply2, threshold, array_t,criteria):
#     o3d.visualization.draw_geometries([target, ply1], window_name='Open3D Result', width=1080,height=720)
#     tree = o3d.geometry.KDTreeFlann(ply2)
#     tree_geo = o3d.geometry.KDTreeFlann.set_geometry(tree,ply2) #建立 ply2 的kdTree 並且set geometry
#     result = getResult(ply1, ply2, tree,threshold, array_t )
#     estimation = TransformationEstimationPointToPoint()
#     # 開始以迭代收斂轉置矩陣-ICP
#     for i in range(criteria.max_iteration_):
#     #for i in range(1):
#         update = estimation.ComputeTransformation(ply1, ply2, result.correspondence_set_)
#         array_t = np.dot(array_t,update)
#
#         print("array_t",array_t)
#         ply1_temp = copy.deepcopy(ply1) # 複製一份ply1
#         ply1_temp.transform(array_t) # 轉換 ply1_temp 轉換矩陣
#         backup = result # 將原本的result 複製一份
#         result = getResult(ply1_temp, ply2, tree,threshold, array_t )
#         if (math.fabs(backup.fitness_ - result.fitness_) < criteria.relative_fitness_ ) and (math.fabs(backup.inlier_rmse_ - result.inlier_rmse_) < criteria.relative_rmse_):
#             break
#     return result

# def getResult(ply1, ply2, kdtree, threshold, array_t): # 只有用到source
#     if threshold <= 0.0:
#         return threshold
#     error2 = 0.0
#     error2_private = 0.0
#     correspondence_set_private=[]
#     # 轉換point cloud 用來計算有幾個點
#     x = np.asarray(ply1.points)
#     y = np.size(x)
#
#     # 宣告一個Registration Result Class
#     result = RegistrationResult(array_t)
#
#     for i in range(int(y/3)):
#         points = ply1.points[i]
#         #ply1.colors[i] = [0,1,0.8]
#         # 尋找鄰近點
#         # k: 在半徑(threshold)內找到幾個鄰近點
#         # vector1 : 搜索到的點coreespondence set
#         # vector2 : 與鄰近點的距離
#         # 1: 最多找1個鄰近點
#         [k,vector1,vector2] = kdtree.search_hybrid_vector_3d(points,threshold,1)
#         if k>0:
#             #ply1.colors[i] = [1,0,0] # 印出有搜索到鄰近點的點-紅色
#             #ply1.colors[vector1[0]] = [0, 1, 0.8] # 印出搜索到的鄰近點-青色
#             error2_private = error2_private + vector2[0]
#             correspondence_set_private.append((i,vector1[0])) # Open3D Vector2iVector型態
#             #print(k,vector1,vector2)
#     #print(len(correspondence_set_private))
#     print("Appending correspondence set")
#     for i in range (len(correspondence_set_private)):
#         result.correspondence_set_.append(correspondence_set_private[i]) # 將找到的鄰近點集合存入correspondence_set_
#     error2 = error2 + error2_private
#
#     # 計算 fitness
#     if len(result.correspondence_set_)==0: # if correspondence set is empty
#         result.fitness_ = 0.0
#         result.inlier_rmse_ = 0.0
#     else:
#         corres_number = len(result.correspondence_set_)
#         result.fitness_ = corres_number/int(y/3)
#         #print("corres_number = ",corres_number)
#         result.inlier_rmse_ = math.sqrt(error2/corres_number)
#     return result

## Call ICP function
trans_result = o3d.pipelines.registration.registration_icp(source, target, threshold, trans_init,o3d.pipelines.registration.TransformationEstimationPointToPoint())
print(trans_result)
print("Transformation is:")
print(trans_result.transformation)
source.transform(trans_result.transformation)
#source.transform(trans_init)
#print(getResult(source,target,o3d.geometry.KDTreeFlann(target),0.02,trans_result.transformation).inlier_rmse_)
#o3d.visualization.draw_geometries([source,target], window_name  ='Open3D Result', width=1080, height=720)
o3d.io.write_point_cloud("source_trasformed.ply", source)
