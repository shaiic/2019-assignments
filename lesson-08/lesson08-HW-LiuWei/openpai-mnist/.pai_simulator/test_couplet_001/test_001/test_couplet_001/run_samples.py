import os
import hdfshelper
import subprocess

print("~~~~~~PAI_JOB_NAME: ", os.environ['PAI_JOB_NAME'])
#jobName = os.environ['PAI_JOB_NAME'].split('~')[1]\
jobName = os.environ['PAI_JOB_NAME']
root_dir = "/couplet_nj/"

print("Downloading data...")
hdfshelper.Download(os.path.join(root_dir, "data"), ".")
hdfshelper.Download(os.path.join(root_dir, "code", "pip_requirements_gpu.txt"), ".")
# hdfshelper.Download(root_dir + "data", ".")
# hdfshelper.Download(root_dir + "code/" + "pip_requirements_gpu.txt", ".")

print("Running...")
subprocess.call("./train.sh", shell=True)

print("Uploading data...")
output_dir = os.path.join(root_dir, "output", jobName)
hdfshelper.Upload("./output/", output_dir)
# hdfshelper.Upload("./data/checkpoint", output_dir)
# hdfshelper.Upload("./data/model.ckpt-200000.data-00000-of-00003", output_dir)
# hdfshelper.Upload("./data/model.ckpt-200000.data-00001-of-00003", output_dir)
# hdfshelper.Upload("./data/model.ckpt-200000.data-00002-of-00003", output_dir)
# hdfshelper.Upload("./data/model.ckpt-200000.index", output_dir)
# hdfshelper.Upload("./data/model.ckpt-200000.meta", output_dir)