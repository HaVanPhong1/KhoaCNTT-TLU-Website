
import axiosClient from './axiosClient'

const lecturerApi = {
    getAll: () => {
        return axiosClient.get("/Lecturers/")
    },

    getById: (id) => {
        return axiosClient.get(`/Lecturers/${id}`)
    },

	create: (data) => {
		return axiosClient.post('/Lecturers', data)
	},

	update: (payload) => {
		const { id, ...body } = payload
		return axiosClient.put(`/Lecturers/${id}`, body)
	},

    delete: (id) => {
        return axiosClient.delete(`/Lecturers/${id}`)
    }
}

export default lecturerApi
