import os
import subprocess

from flask import Flask, request, send_file
from pathlib import Path

app = Flask(__name__)

@app.route('/registration', methods=['POST'])
def registration():
    files = request.files.getlist("file")
    ext = Path(files[0].filename).suffix
    for file in files:
        file.save(file.filename)

    output = Path(files[0].filename).stem+"_"+Path(files[1].filename).stem+ext
    p = subprocess.run(
        [
            'python', 'open3d_ICP_ori_v2.py', 
            files[0].filename, 
            files[1].filename, 
            output
        ]
    )
    return send_file(output, as_attachment=True)


if __name__ == "__main__":
    app.run(debug=True, host='0.0.0.0', port=8888)