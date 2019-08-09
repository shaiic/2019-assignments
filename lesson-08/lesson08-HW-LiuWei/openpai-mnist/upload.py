import os
import hdfshelper
jobName = os.environ['PAI_JOB_NAME']
root_dir = "/resource/"
print("Uploading data...")
output_dir = os.path.join(root_dir, "output", jobName)
hdfshelper.Upload("./output/", output_dir)
output_dir = os.path.join(root_dir, "export", jobName)
hdfshelper.Upload("./export/", output_dir)