import json
import numpy as np
import pandas as pd
import os
import tensorflow as tf
from tensorflow.keras.models import Sequential
from tensorflow.keras.layers import Activation
from tensorflow.keras.optimizers import SGD
from tensorflow.keras.layers import Dense, Dropout, Flatten
from tensorflow.keras.layers import Conv2D, MaxPooling2D
from tensorflow.keras.layers import Input, Dense, Conv2D, MaxPooling2D, UpSampling2D
from tensorflow.keras.models import Model
from tensorflow.keras.preprocessing import image
from tensorflow.keras.models import Sequential
from tensorflow.keras.layers import Dense, Dropout, Activation, BatchNormalization
from tensorflow.keras.applications.vgg19 import VGG19
from tensorflow.keras.applications.vgg19 import preprocess_input, decode_predictions
from tensorflow.keras.preprocessing.text import Tokenizer
from tensorflow.keras.utils import to_categorical
from tensorflow.keras.losses import categorical_crossentropy, binary_crossentropy, cosine_similarity
from tensorflow.keras import backend as K
from tensorflow.keras.callbacks import EarlyStopping, ModelCheckpoint


from tensorflow.keras.layers import Input, Dense, Conv2D, MaxPooling2D, UpSampling2D, Conv2DTranspose
from tensorflow.keras.models import Model


from azureml.core.model import Model
from inference_schema.schema_decorators import input_schema, output_schema
from inference_schema.parameter_types.numpy_parameter_type import NumpyParameterType

series_model = None

def init():
    global series_model

    series_model_root = Model.get_model_path('EngineFailurePrediction')
    print('Series Model root:', series_model_root)
    #series_model_file = os.path.join(series_model_root, 'model')
    #print('Series Model file:', series_model_file)
    series_model = tf.keras.models.load_model(series_model_root)
    series_model.compile(loss = ('binary_crossentropy'), optimizer='adam', metrics=['acc'])
    print(series_model.summary())


input_sample = np.random.rand(30,24)
output_sample = np.array([0])


@input_schema('data', NumpyParameterType(input_sample))
@output_schema(NumpyParameterType(output_sample))
def run(data):
    print(data)
    data = np.array(json.loads(data)['data'])
    img_path = data[0]
    print('First url:', img_path)
    img = iio.load_image_from_url(img_path, image_size=(128,256), convert_to_grey=True, keep_3d_shape=True)
    input = np.array([img])

    # make prediction
    time_series = series_model.predict(input)


    p = pd.DataFrame(np.transpose(time_series), columns=['Close'])
    p['moving'] = p['Close'].transform(lambda x: x.rolling(30, 30).mean())
    input2 = np.array(p.tail(30)[['moving', 'Close']]).reshape(1, 60)

    stage = type_model.predict(input2)
    stage = np.argmax(stage, axis=1)[0]
    return {'stage': str(stage), 'series': time_series.tolist()}

