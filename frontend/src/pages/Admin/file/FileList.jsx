import { useEffect, useState } from 'react'
import fileApi from '../../../api/fileApi'
import studentApi from '../../../api/studentApi'
import DataTable from '../../../components/table/DataTable'
import IconButton from '../../../components/parts/IconButton'
import FormModal from '../../../components/modal/FormModal'
import FilterForm from '../../../components/parts/FilterForm'
import Pagination from '../../../components/table/Pagination'
import PopupMessage from '../../../components/parts/PopupMessage'
import {
	columns,
	uploadFields,
	replaceFields,
	editMetadataFields,
	getSearchConfig
} from '../../../constants/file'
import { checkSize } from '../../../helpers/fileHelpers'
import { Replace, Download, Trash2, Pencil } from 'lucide-react'

function FileList() {
	const [files, setFiles] = useState([])
	const [subjects, setSubjects] = useState([])
	const [showUpload, setShowUpload] = useState(false)

	// Các trạng thái Modal
	const [replaceFile, setReplaceFile] = useState(null)
	const [editFile, setEditFile] = useState(null)
	const [popup, setPopup] = useState(null)
	const [warning, setWarning] = useState(null)

	const [page, setPage] = useState(1)
	const [totalPages, setTotalPages] = useState(0)
	const [filters, setFilters] = useState({
		keyword: '',
		subjectCodes: [],
		fileType: '',
		pageSize: 10
	})

	useEffect(() => {
		studentApi.getSubjects().then(setSubjects).catch(console.error)
	}, [])

	const loadFiles = async () => {
		try {
			const res = await fileApi.search({ ...filters, page })
			setFiles(res.items)
			setTotalPages(Math.ceil(res.total / filters.pageSize))
		} catch (err) {
			const msg =
				err.response?.data?.message ||
				err.response?.data?.error ||
				err.response?.data?.detail ||
				err.message ||
				'Không thể kết nối đến máy chủ, thử lại sau.'
			setPopup?.(msg)
		}
	}

	useEffect(() => {
		const loadFiles = async () => {
			try {
				const res = await fileApi.search({ ...filters, page })
				setFiles(res.items)
				setTotalPages(Math.ceil(res.total / filters.pageSize))
			} catch (err) {
				const msg =
					err.response?.data?.message ||
					err.response?.data?.error ||
					err.response?.data?.detail ||
					err.message ||
					'Không thể kết nối đến máy chủ, thử lại sau.'
				setPopup?.(msg)
			}
		}
		loadFiles()
	}, [page, filters])

	return (
		<div>
			<div className='flex justify-between items-center mb-6'>
				<h2 className='text-xl font-bold text-gray-800'>
					Quản lý Tài liệu
				</h2>

				<button
					className='bg-[#1f4c7a] text-white px-5 py-2.5 rounded-lg hover:bg-[#163a5d] transition shadow-sm font-medium text-sm'
					onClick={() => setShowUpload(true)}>
					Thêm tài liệu mới
				</button>
			</div>

			<FilterForm
				fields={getSearchConfig(subjects)}
				onSearch={(values) => {
					setFilters(values)
					setPage(1)
				}}
			/>
			<div className='my-4'>
				<Pagination
					page={page}
					totalPages={totalPages}
					setPage={setPage}
				/>
			</div>

			<DataTable
				columns={columns}
				data={files}
				actions={(row) => (
					<>
						<IconButton
							icon={Pencil}
							color='gray'
							onClick={() => setEditFile(row)}
						/>
						<IconButton
							icon={Replace}
							color='blue'
							onClick={() => setReplaceFile(row)}
						/>
						<IconButton
							icon={Download}
							color='green'
							onClick={() => fileApi.download(row.id)}
						/>
						<IconButton
							icon={Trash2}
							color='red'
							onClick={() =>
								setWarning({
									title: 'Xác nhận xóa tài liệu',
									message:
										'Bạn có chắc chắn muốn xóa tài liệu này?',
									action: () => fileApi.delete(row.id),
									popup: 'Xóa tài liệu thành công.'
								})
							}
						/>
					</>
				)}
			/>

			{/* Modal Upload */}
			{showUpload && (
				<FormModal
					title='Thêm tài liệu'
					fields={uploadFields}
					columns={2}
					width='600px'
					onSubmit={async (formData) => {
						try {
							const file = formData.get('file')
							if (!file) {
								setPopup('Vui lòng chọn file.')
								return
							}
							const error = checkSize(file, '250MB')
							if (!error) {
								setPopup(
									'Tài liệu tải lên không được nặng hơn 250MB.'
								)
								return
							}
							const res = await fileApi.upload(formData)

							setPopup(res.message)

							setShowUpload(false)
							loadFiles()
						} catch (err) {
							const msg =
								err.response?.data?.message ||
								err.response?.data?.error ||
								err.response?.data?.detail ||
								err.message ||
								'Không thể kết nối đến máy chủ, thử lại sau.'
							setPopup?.(msg)
						}
					}}
					onClose={() => setShowUpload(false)}
				/>
			)}

			{/* Modal Edit Metadata (Sửa tiêu đề, quyền, môn học) */}
			{editFile && (
				<FormModal
					title='Sửa thông tin tài liệu'
					fields={editMetadataFields}
					defaultValues={editFile}
					onSubmit={async (formData) => {
						try {
							const data = Object.fromEntries(formData.entries())

							if (!data.subjectCode) {
								delete data.subjectCode
							}
							await fileApi.updateMetadata(editFile.id, data)

							setPopup('Cập nhật thông tin tài liệu thành công.')
							setEditFile(null)
							loadFiles()
						} catch (err) {
							const msg =
								err.response?.data?.message ||
								err.response?.data?.error ||
								err.response?.data?.detail ||
								err.message ||
								'Không thể kết nối đến máy chủ, thử lại sau.'
							setPopup?.(msg)
						}
					}}
					onClose={() => setEditFile(null)}
				/>
			)}

			{/* Modal Replace */}
			{replaceFile && (
				<FormModal
					title='Đổi tài liệu'
					fields={replaceFields}
					defaultValues={{ oldTitle: replaceFile.title }}
					onSubmit={async (formData) => {
						try {
							formData.append('Title', replaceFile.title)
							formData.append(
								'SubjectCode',
								replaceFile.subjectCode || ''
							)
							formData.append('FileType', replaceFile.fileType)
							formData.append(
								'Permission',
								replaceFile.permission
							)
							const file = formData.get('file')

							const error = checkSize(file, '250MB')
							if (error) {
								setPopup(
									'Tài liệu tải lên không được nặng hơn 250MB.'
								)
								return
							}
							const res = await fileApi.replace(
								replaceFile.id,
								formData
							)
							setPopup(res.message)
							setReplaceFile(null)
							loadFiles()
						} catch (err) {
							const msg =
								err.response?.data?.message ||
								err.response?.data?.error ||
								err.response?.data?.detail ||
								err.message ||
								'Không thể kết nối đến máy chủ, thử lại sau.'
							setPopup?.(msg)
						}
					}}
					onClose={() => setReplaceFile(null)}
				/>
			)}

			{popup && (
				<PopupMessage message={popup} onClose={() => setPopup(null)} />
			)}

			{warning && (
				<ConfirmModal
					title={warning.title}
					message={warning.message}
					onConfirm={async () => {
						try {
							await warning.action()
							setPopup(warning.popup)
							setWarning(null)
							loadFiles()
						} catch (err) {
							const msg =
								err.response?.data?.message ||
								err.response?.data?.error ||
								err.response?.data?.detail ||
								err.message ||
								'Không thể kết nối đến máy chủ, thử lại sau.'
							setPopup?.(msg)
						}
					}}
					onClose={() => setWarning(null)}
					confirmText='Xác nhận'
				/>
			)}
		</div>
	)
}

export default FileList
