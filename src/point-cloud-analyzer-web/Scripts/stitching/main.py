import os
import argparse
import progressbar
import collections
import numpy as np
import open3d as o3d
import sys

def custom_draw_geometry_with_rotation(pcd):

    def rotate_view(vis):
        ctr = vis.get_view_control()
        ctr.rotate(30.0, 0.0)
        return False

    o3d.visualization.draw_geometries_with_animation_callback(pcd,
                                                              rotate_view)
                                                              
#IO utils:
import utils.io as io
import utils.visualize as visualize
import matplotlib.pyplot as plt
from pathlib import Path
import csv

#import iss feature detection module
from iss import  detect

from  RANSAC import ransac_match
from  ICP    import icp_exact_match
from RANSAC import DE_match
from  global_rigistation import global_rigistration
import time                                                         
#RANSAC configuration:
RANSACParams = collections.namedtuple(
    'RANSACParams',
    [
        'max_workers',
        'num_samples',
        'max_correspondence_distance', 'max_iteration', 'max_validation', 'max_refinement'
    ]
)
# fast pruning algorithm configuration:
CheckerParams = collections.namedtuple(
    'CheckerParams',
    ['max_correspondence_distance', 'max_edge_length_ratio', 'normal_angle_threshold']
)


def start_rigistration(pcd,R):
        radius = 0.5
        pcd_source = o3d.io.read_point_cloud(pcd)
        pcd_target = o3d.io.read_point_cloud(pcd)

        # trans_init = np.asarray([[0, 0, 1, 0],
        #                          [1, 0, 0, 0],
        #                          [0, 1, 0, 0],
        #                          [0, 0, 0, 1]])

        trans_init = np.asarray([[1, 0, 0, 0],
                                 [0, 1, 0, 0],
                                 [0, 0, 1, 0],
                                 [0, 0, 0, 1]])
        #trans_init[:3, :3] = mesh.get_rotation_matrix_from_xyz((0, 0.7 * np.pi , 0))
        #pcd_source = pcd_source.transform(trans_init)
        pcd_source = pcd_source.rotate(R)
        custom_draw_geometry_with_rotation([pcd_source,pcd_target])
        #o3d.visualization.draw_geometries([pcd_target,pcd_source],zoom=1,
        #                              front=[0.6452, -0.3036, -1.7011],
        #                              lookat=[1.9892, 2.0208, 1.8945],
        #                              up=[0.2779, -2.9482, 0.1556])
        
        # step2 对点云进行降采样downsample
        pcd_source_down = pcd_source.voxel_down_sample(voxel_size=0.2)
        pcd_target_down = pcd_target.voxel_down_sample(voxel_size=0.2)
        print("hi")
        ## 构建 kd-tree
        search_tree_source = o3d.geometry.KDTreeFlann(pcd_source_down)
        search_tree_target = o3d.geometry.KDTreeFlann(pcd_target_down)
        print("123456789")
        # step3 iss 特征点提取
        keypoints_source = detect(pcd_source_down, search_tree_source, 0.2)
        print("987654312")
        keypoints_target = detect(pcd_target_down, search_tree_target, 0.2)
        print("hello")

        # step4 fpfh特征点描述 feature description
        pcd_source_keypoints = pcd_source.select_by_index(keypoints_source['id'].values)
        ##fpfh 进行 特征点描述
        fpfh_source_keypoints = o3d.pipelines.registration.compute_fpfh_feature(
            pcd_source_keypoints,
            o3d.geometry.KDTreeSearchParamHybrid(radius=radius * 5, max_nn=100)
        ).data
        pcd_target_keypoints = pcd_target.select_by_index(keypoints_target['id'].values)
        fpfh_target_keypoints = o3d.pipelines.registration.compute_fpfh_feature(
            pcd_target_keypoints,
            o3d.geometry.KDTreeSearchParamHybrid(radius=radius * 5, max_nn=100)
        ).data

        # generate matches:
        distance_threshold_init = 1.5 * radius
        distance_threshold_final = 1.0 * radius
        # step5 RANSAC Registration,初配准、得到初始旋转、平移矩阵
        init_result = DE_match(
            idx_target, idx_source,
            pcd_source_keypoints, pcd_target_keypoints,
            fpfh_source_keypoints, fpfh_target_keypoints,
            ransac_params=RANSACParams(
                max_workers=5,
                num_samples=4,
                max_correspondence_distance=distance_threshold_init,
                max_iteration=200000,
                max_validation=20,
                max_refinement=30
            ),
            checker_params=CheckerParams(
                max_correspondence_distance=distance_threshold_init,
                max_edge_length_ratio=0.9,
                normal_angle_threshold=None
            )
        )
        # step6 ICP for refined estimation,优化配准 ICP对初始T进行迭代优化:
        final_result = icp_exact_match(
            pcd_source_down, pcd_target_down, search_tree_target,
            init_result.transformation,
            distance_threshold_final, 60
        )
        # visualize:
        point_result = visualize.show_registration_result(
            pcd_source_keypoints, pcd_target_keypoints, init_result.correspondence_set,
            pcd_source, pcd_target, final_result.transformation
        )
        #
        # # add result:
        # io.add_to_output(df_output, idx_target, idx_source, final_result.transformation)
        #
        # # write output:
        # io.write_output(
        #     os.path.join(input_dir, 'reg_result_yaogefad.txt'),
        #     df_output
        # )
        print('point result is  ',point_result)
        return point_result
        


if __name__ == '__main__':
    radius = 0.5
    #step1 Read Point Cloud data
    input_dir  = "/app/Scripts/stitching/registration_dataset"
    registration_results = io.read_registration_results(os.path.join(input_dir,'reg_result.txt')) #读取 reg_result.txt 结果
    ##init output
    df_output = io.init_output()
    ## Batch read data
    # for i,r  in (
    #         list(registration_results.iterrows())
    # ):
    #     idx_target = int(r['idx1'])
    #     idx_source = int(r['idx2'])
    idx_target = 643
    idx_source = 456
    ##load point cloud
    # pcd_source = io.read_point_cloud_bin(
    #     os.path.join(input_dir,'point_clouds',f'{idx_source}.bin')
    # )
    # pcd_target = io.read_point_cloud_bin(
    #     os.path.join(input_dir,'point_clouds',f'{idx_target}.bin')
    # )

    ######寫檔案
    if not os.path.isdir('/app/Scripts/stitching/registration_result/'):
        os.mkdir('/app/Scripts/stitching/registration_result/')
    else:
        print(".")
    path = '/app/Scripts/stitching/registration_result/result/'
    if not os.path.isdir(path):
        os.mkdir(path)
    else:
        print(".")
    ##檔案是否存在
    file_count=0

    while os.path.isfile(path+'result_'+str(file_count)+'.csv'):
        print("檔案存在")
        file_count +=1
    file_name = Path(path + 'result_' + str(file_count) + '.csv')
    file_name.touch(exist_ok=True)
    f = open(file_name, "a", newline="")
    file = csv.writer(f)
    file.writerow(['Fit', 'RMSE'])
    f.close()

    pcd_source = io.read_point_cloud_bin('/app/Scripts/stitching/registration_dataset/point_clouds/643.bin')
    pcd_target = io.read_point_cloud_bin('/app/Scripts/stitching/registration_dataset/point_clouds/456.bin')

    ##批量读取
    # for i,r  in (
    #         list(registration_results.iterrows())
    # ):
    #     idx_target = int(r['idx1'])
    #     idx_source = int(r['idx2'])
    idx_target = 643
    idx_source = 456

    ##load point cloud
    # pcd_source = io.read_point_cloud_bin(
    #     os.path.join(input_dir,'point_clouds',f'{idx_source}.bin')
    # )
    # pcd_target = io.read_point_cloud_bin(
    #     os.path.join(input_dir,'point_clouds',f'{idx_target}.bin')
    # )
    
    #####################################################  放入pcd,ply檔在此
    pcds = [sys.argv[1],sys.argv[2],sys.argv[3],sys.argv[4],sys.argv[5]]
    mesh = o3d.geometry.TriangleMesh.create_coordinate_frame()
    
    #選轉處理
    R = mesh.get_rotation_matrix_from_xyz((0,5*np.pi/9,0))
    result_0 = start_rigistration(pcds[0],R)
    R = mesh.get_rotation_matrix_from_yxz((0,5*np.pi/9,0))
    result_1 = start_rigistration(pcds[1],R)
    R = mesh.get_rotation_matrix_from_xzy((0,5*np.pi/9,0))
    result_2 = start_rigistration(pcds[2],R)
    R = mesh.get_rotation_matrix_from_yxz((0,5*np.pi/9,0))
    result_3 = start_rigistration(pcds[3],R)
    R = mesh.get_rotation_matrix_from_xzy((0,5*np.pi/9,0))
    result_4 = start_rigistration(pcds[4],R)
    
    #畫取新的點雲檔
    o3d.io.write_point_cloud('new_result_0.ply',result_0)
    o3d.io.write_point_cloud('new_result_1.ply',result_1)
    o3d.io.write_point_cloud('new_result_2.ply',result_2)
    o3d.io.write_point_cloud('new_result_3.ply',result_3)
    o3d.io.write_point_cloud('new_result_4.ply',result_4)
    
    pcd = ['new_result_0.ply','new_result_1.ply','new_result_2.ply','new_result_3.ply','new_result_4.ply']
    ##result 為最終拼接點雲檔
    result = global_rigistration(pcd)
    
    ########################################################
    
    
    