import os
import subprocess

from flask import Flask, request, Response

app = Flask(__name__)

@app.route('/convert', methods=['POST'])
def convert():
    data = request.form
    print(data)
    os.makedirs('~/code/OpenSfM/data/'+data['command'], exist_ok=True)
    files = request.files.getlist("file")
    print(files)
    for file in files:
        file.save('~/code/OpenSfM/data/'+data['command']+'/'+file.filename)

    # copy 
    p = subprocess.run(
        [
            'docker', 'exec', '-it', 'opensfm_opensfm_1', 'bin/opensfm_run_all', '~/code/OpenSfM/data/'+data['command']
        ]
    )
    return Response(status = 200)


if __name__ == "__main__":
    app.run(debug=True, host='0.0.0.0', port=8001)